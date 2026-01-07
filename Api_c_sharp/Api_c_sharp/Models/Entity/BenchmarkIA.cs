using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_c_sharp.Models.Entity;

[Table("t_e_benchmarksia_ben")]
public class BenchmarkIA
{
    [Key]
    [Column("ben_id_benchmark")]
    public int IdBenchmark { get; set; }

    [Column("ben_benchmark_id")]
    [Required]
    [MaxLength(100)]
    public string BenchmarkId { get; set; } = string.Empty;

    [Column("ben_model_type")]
    [Required]
    [MaxLength(50)]
    public string ModelType { get; set; } = string.Empty;

    [Column("ben_timestamp")]
    [Required]
    public DateTime Timestamp { get; set; }

    [Column("ben_total_iterations")]
    public int TotalIterations { get; set; }

    [Column("ben_successful_predictions")]
    public int SuccessfulPredictions { get; set; }

    [Column("ben_failed_predictions")]
    public int FailedPredictions { get; set; }

    [Column("ben_success_rate_percent")]
    public double SuccessRatePercent { get; set; }

    [Column("ben_avg_inference_time_ms")]
    public double AvgInferenceTimeMs { get; set; }

    [Column("ben_min_inference_time_ms")]
    public double MinInferenceTimeMs { get; set; }

    [Column("ben_max_inference_time_ms")]
    public double MaxInferenceTimeMs { get; set; }

    [Column("ben_std_inference_time_ms")]
    public double StdInferenceTimeMs { get; set; }

    [Column("ben_predictions_per_second")]
    public double PredictionsPerSecond { get; set; }

    [Column("ben_total_time_seconds")]
    public double TotalTimeSeconds { get; set; }

    [Column("ben_platform")]
    [MaxLength(200)]
    public string? Platform { get; set; }

    [Column("ben_processor")]
    [MaxLength(100)]
    public string? Processor { get; set; }

    [Column("ben_python_version")]
    [MaxLength(50)]
    public string? PythonVersion { get; set; }

    [Column("ben_cpu_count")]
    public int? CpuCount { get; set; }

    [Column("ben_memory_total_gb")]
    public double? MemoryTotalGb { get; set; }

    [Column("ben_memory_available_gb")]
    public double? MemoryAvailableGb { get; set; }
}