using Contracts.Dtos.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces.Shared
{
    public interface IService<in TIn, TOut>
    {
        Task<ResultDto<TOut>> ExecuteServiceAsync(TIn inputDto, CancellationToken cancellationToken = default);
    }
}
