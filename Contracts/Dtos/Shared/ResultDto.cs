namespace Contracts.Dtos.Shared
{
    public class ResultDto<TOut>
    {
        public TOut Data { get; set; }

        public HashSet<ErrorDto> Errors { get; set; }

        public bool IsValid { get; set; }

        public ResultDto()
        {
            Errors = [];
            IsValid = true;
        }

        public ResultDto(params ErrorDto[] errors)
        {
            Data = default;
            IsValid = false;
            Errors = new HashSet<ErrorDto>(errors);
        }
    }
}
