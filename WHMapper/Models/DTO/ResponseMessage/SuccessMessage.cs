namespace WHMapper.Models.DTO.ResponseMessage
{
    public class SuccessMessage<T> : IResponseMessage
    {
        public T Response { get; private set; }

        public SuccessMessage(T responseMsg)
        {
            Response = responseMsg;
        }

    }
}
