
namespace MT.Infra.Common
{
   
    public interface IBaseContract
    {
         string Message { get; set; }
         Status Status { get; set; }
    }

    public enum Status
    {
        Failure,
        Success,
        Warning
    }

}
