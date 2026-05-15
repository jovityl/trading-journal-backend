using MediatR;

namespace TradingJournal.Contract.Abstraction
{
    public interface IQuery<TResponse> : IRequest<TResponse>
    {
    }
}
