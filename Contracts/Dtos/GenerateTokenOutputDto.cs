using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Dtos
{
	public class GenerateTokenOutputDto
	{
        public UserDto User { get; set; }
        public string Token { get; set; }
    }
}
