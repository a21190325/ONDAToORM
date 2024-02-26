using Contracts.Dtos;
using Contracts.Dtos.Shared;
using Contracts.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
    public interface IConverterService : IService<ConverterInputDto, List<FileDto>>
    {
    }
}
