using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Dtos.Shared
{
    public class ResultDto<TOut>
    {
        public TOut Data { get; set; }

        public HashSet<ErrorDto> Errors { get; set; }

        public bool IsValid { get; set; }

        public ResultDto()
        {
            Errors = new HashSet<ErrorDto>();
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
