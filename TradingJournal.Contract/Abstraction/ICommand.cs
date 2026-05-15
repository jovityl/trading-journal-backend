using MediatR;

namespace TradingJournal.Contract.Abstraction
{
    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }
}
