using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenUtau.Core.Util;

namespace OpenUtauMobile.Utils {
    public class DeepSeekService {
        private static readonly Lazy<DeepSeekService> _instance = new Lazy<DeepSeekService>(() => new DeepSeekService());
        public static DeepSeekService Instance => _instance.Value;

        private readonly HttpClient _httpClient;

        private DeepSeekService() {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// 获取系统提示词，包含编曲指令协议的完整描述
        /// </summary>
        private string GetFullSystemPrompt() {
            var basePrompt = Preferences.Default.DeepSeekSystemPrompt;
            if (string.IsNullOrWhiteSpace(basePrompt)) {
                basePrompt = "你是一个编曲助手，根据用户需求生成 JSON 指令操作编曲软件。";
            }

            return $@"{basePrompt}

你必须用以下 JSON 格式回复，不要包含任何其他内容：
{{
  ""actions"": [
    {{
      ""type"": ""操作类型"",
      // 操作参数...
    }}
  ],
  ""message"": ""给用户的文字回复（可选）""
}}

支持的操作类型及参数：
1. add_notes - 添加音符
   track: 轨道号（从0开始，可选，默认当前轨道）
   notes: [
     {{ position: int（位置，tick单位，480=1拍）, tone: int（音高，60=C4）, duration: int（长度，tick单位，480=1拍）, lyric: string（歌词，可选，默认""la""） }}
   ]

2. remove_notes - 删除音符
   track: 轨道号
   part_index: 分片索引
   note_indices: [int]（要删除的音符索引列表）
   clear_all: true（清空该分片所有音符，可选）

3. modify_notes - 修改音符
   方式一（推荐，可一次改多个）：
   modifications: [
     {{ index: int（要修改的音符索引，从0开始）, tone: int（新音高，可选）, duration: int（新长度，可选）, lyric: string（新歌词，可选） }}
   ]
   方式二（一次只改一个）：
   part_index: 分片索引
   note_index: int（要修改的音符索引）
   tone: int（新音高，可选）
   duration: int（新长度，可选）
   lyric: string（新歌词，可选）

4. set_bpm - 设置BPM
   bpm: double（速度值）

5. set_beat - 设置拍号
   beat_numerator: int（分子，如4）
   beat_denominator: int（分母，如4）

6. add_track - 添加轨道
   track_name: string（轨道名，可选）
   singer_name: string（歌手名，可选）

7. select_part - 选中分片
   track: 轨道号
   select_part_index: int（分片索引）

8. message - 仅文字回复，不执行操作

音符位置和时间单位：1四分音符=480tick，1小节=1920tick（4/4拍时）。
音高：60=C4, 61=C#4, 62=D4...
默认BPM=120。";
        }

        /// <summary>
        /// 发送聊天请求到 DeepSeek API
        /// </summary>
        public async Task<string> SendChatAsync(string userMessage, string? apiKey = null, string? endpoint = null, string? modelName = null) {
            var key = apiKey ?? Preferences.Default.DeepSeekApiKey;
            var ep = endpoint ?? Preferences.Default.DeepSeekEndpoint;
            var model = modelName ?? Preferences.Default.DeepSeekModelName;

            if (string.IsNullOrWhiteSpace(key)) {
                throw new InvalidOperationException("请先在设置中填写 DeepSeek API Key");
            }
            if (string.IsNullOrWhiteSpace(ep)) {
                throw new InvalidOperationException("请先在设置中填写 API 端点");
            }

            var request = new DeepSeekChatRequest {
                Model = model,
                Messages = new System.Collections.Generic.List<DeepSeekMessage> {
                    new DeepSeekMessage { Role = "system", Content = GetFullSystemPrompt() },
                    new DeepSeekMessage { Role = "user", Content = userMessage }
                },
                Temperature = 0.7,
                MaxTokens = 4096
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");

            var response = await _httpClient.PostAsync(ep, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) {
                throw new HttpRequestException($"API 请求失败 ({(int)response.StatusCode}): {responseBody}");
            }

            var chatResponse = JsonSerializer.Deserialize<DeepSeekChatResponse>(responseBody);
            if (chatResponse?.Choices == null || chatResponse.Choices.Count == 0) {
                throw new Exception("API 返回为空，请重试");
            }

            var reply = chatResponse.Choices[0].Message?.Content ?? string.Empty;

            // 尝试提取 JSON（AI 可能用 ```json ``` 包裹）
            if (reply.Contains("```")) {
                var start = reply.IndexOf("```json") + 7;
                if (start < 7) start = reply.IndexOf("```") + 3;
                var end = reply.LastIndexOf("```");
                if (start >= 0 && end > start) {
                    reply = reply.Substring(start, end - start).Trim();
                }
            }

            return reply;
        }

        /// <summary>
        /// 解析 AI 回复中的编曲指令
        /// </summary>
        public AiCommandRoot? ParseCommands(string reply) {
            // 尝试提取 JSON（AI 可能用 ```json ``` 包裹或夹带文字）
            string jsonText = reply;
            var start = jsonText.IndexOf('{');
            var end = jsonText.LastIndexOf('}');
            if (start >= 0 && end > start) {
                jsonText = jsonText.Substring(start, end - start + 1);
            }
            try {
                return JsonSerializer.Deserialize<AiCommandRoot>(jsonText);
            } catch (Exception ex) {
                throw new FormatException($"AI 返回的不是有效指令 JSON：{reply}", ex);
            }
        }
    }
}
