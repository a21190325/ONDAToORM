namespace Contracts.Dtos.Shared
{
    public class ErrorDto
    {
        public string? Code { get; set; }
        public string? Field { get; set; }
        public HashSet<string?> Values { get; set; }

        public ErrorDto()
        {
            Values = [];
        }

        public ErrorDto(string code)
        {
            Code = code;
            Values = [];
        }

        public ErrorDto(string code, string? field) : this(code)
        {
            Field = field;
        }

        public ErrorDto(string code, string? field = default, params string[] values) : this(code, field)
        {
            if (values != null)
            {
                Values = new HashSet<string?>(values);
            }
        }

        public ErrorDto(string code, string? field = default, IEnumerable<string?>? values = default) : this(code, field)
        {
            if (values != null)
            {
                Values = new HashSet<string?>(values);
            }
        }
    }
}
