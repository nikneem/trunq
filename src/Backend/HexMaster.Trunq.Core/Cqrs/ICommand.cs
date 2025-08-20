namespace HexMaster.Trunq.Core.Cqrs;

public interface ICommand
{
    Guid CommandId { get; }
}