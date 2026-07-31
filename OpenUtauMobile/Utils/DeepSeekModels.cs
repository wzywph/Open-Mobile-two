using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenUtauMobile.Utils {
    // ===== API 请求模型 =====
    public class DeepSeekChatRequest {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "deepseek-v4-flash";

        [JsonPropertyName("messages")]
        public List<DeepSeekMessage> Messages { get; set; } = new List<DeepSeekMessage>();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 4096;
    }

    public class DeepSeekMessage {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    // ===== API 响应模型 =====
    public class DeepSeekChatResponse {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<DeepSeekChoice> Choices { get; set; } = new List<DeepSeekChoice>();

        [JsonPropertyName("usage")]
        public DeepSeekUsage? Usage { get; set; }
    }

    public class DeepSeekChoice {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public DeepSeekMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }

    public class DeepSeekUsage {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    // ===== AI 编曲指令协议 =====
    public class AiCommandRoot {
        [JsonPropertyName("actions")]
        public List<AiCommandAction> Actions { get; set; } = new List<AiCommandAction>();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class AiCommandAction {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("track")]
        public int? Track { get; set; }

        // add_notes 参数
        [JsonPropertyName("notes")]
        public List<AiNoteData>? Notes { get; set; }

        // remove_notes 参数
        [JsonPropertyName("note_indices")]
        public List<int>? NoteIndices { get; set; }

        [JsonPropertyName("part_index")]
        public int? PartIndex { get; set; }

        [JsonPropertyName("clear_all")]
        public bool? ClearAll { get; set; }

        // modify_notes 参数
        [JsonPropertyName("note_index")]
        public int? NoteIndex { get; set; }

        [JsonPropertyName("tone")]
        public int? Tone { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("lyric")]
        public string? Lyric { get; set; }

        // set_bpm 参数
        [JsonPropertyName("bpm")]
        public double? Bpm { get; set; }

        // set_beat 参数
        [JsonPropertyName("beat_numerator")]
        public int? BeatNumerator { get; set; }

        [JsonPropertyName("beat_denominator")]
        public int? BeatDenominator { get; set; }

        // add_track 参数
        [JsonPropertyName("track_name")]
        public string? TrackName { get; set; }

        [JsonPropertyName("singer_name")]
        public string? SingerName { get; set; }

        // select_part 参数
        [JsonPropertyName("select_part_index")]
        public int? SelectPartIndex { get; set; }

        // modify_notes 批量修改参数（一次改多个音符）
        [JsonPropertyName("modifications")]
        public List<AiNoteModification>? Modifications { get; set; }
    }

    public class AiNoteData {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("tone")]
        public int Tone { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("lyric")]
        public string Lyric { get; set; } = "la";
    }

    // ===== modify_notes 批量修改专用（一次改多个音符） =====
    public class AiNoteModification {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("tone")]
        public int? Tone { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("lyric")]
        public string? Lyric { get; set; }
    }
}
