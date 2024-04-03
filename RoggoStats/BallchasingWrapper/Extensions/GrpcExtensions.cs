namespace BallchasingWrapper.Extensions;

public static class GrpcExtensions
{
    public static (DateTime begin, DateTime end) ToDateTimes(this Grpc.TimeRange timeRange)
    {
        switch (timeRange)
        {
            case Grpc.TimeRange.EveryTime:
                return (DateTime.MinValue, DateTime.MaxValue);
            case Grpc.TimeRange.Today:
                return (DateTime.Today, DateTime.Today.AddDays(1));
            case Grpc.TimeRange.Yesterday:
                return (DateTime.Today.Subtract(TimeSpan.FromDays(1)), DateTime.Today);
            case Grpc.TimeRange.Week:
                return (DateTime.Today.Subtract(TimeSpan.FromDays(7)), DateTime.Today.AddDays(1));
            case Grpc.TimeRange.Month:
                return (DateTime.Today.Subtract(TimeSpan.FromDays(30)), DateTime.Today.AddDays(1));
            case Grpc.TimeRange.Year:
                return (DateTime.Today.Subtract(TimeSpan.FromDays(365)), DateTime.Today.AddDays(1));
            default:
                throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null);
        }
    }
}