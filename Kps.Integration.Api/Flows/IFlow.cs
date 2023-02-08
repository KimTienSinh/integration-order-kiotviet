namespace Kps.Integration.Api.Flows
{
    public interface IFlow<TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput inputData);
    }
}
