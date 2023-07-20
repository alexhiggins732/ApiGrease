using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebApiClient
{
    internal class ControllerProxy
    {
    }

    [ApiController]
    public class ApiControllerBase<T>
    {
        protected ILoggerFactory loggerFactory;
        protected IConfiguration configuration;

        public ApiControllerBase(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            this.loggerFactory = loggerFactory;
            this.configuration = configuration;
        }
        protected async Task<ApiResponse<TResult>> GetResult<TResult>
            (
                Func<Task<TResult>> act, [CallerMemberName] string? caller = null
            )
        {
            var result = await act();
            return new ApiResponse<TResult>(result);
        }
    }
    public class ApiResponse<TResult>
    {
        public TResult? Result { get; set; }
        public bool? HasError { get; set; }
        public string Message { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public ApiResponse(TResult? result, string message = "success", bool? hasError = null, List<string>? errorMessages = null)
        {
            Result = result;
            HasError = hasError;
            Message = message;
            ErrorMessages = errorMessages;
        }
    }
    public interface ITestController
    {

    }

    public class DtoModel<T>
    {

        public T Request { get; set; } = default!;
    }
    public class Model1AddRequest : DtoModel<DomainModel1> { }
    public class Model2AddRequest : DtoModel<DomainModel2> { }
    public class Model1UpdateRequest : DtoModel<DomainModel1> { }
    public class Model2UpdateRequest : DtoModel<DomainModel2> { }
   
    public class DomainBaseModel
    {
        public int Id { get; set; }
    }
    public class DomainModel1: DomainBaseModel { public string Name { get; set; } = null!; }
    public class DomainModel2: DomainBaseModel { }
    
    public class TestIdProvider
    {
        private static ConcurrentDictionary<Type, int> testIds = new();
        public static int NextId<T>()
            => testIds.AddOrUpdate(typeof(T), key=> 1, (key, src) => src++);

    }

    public class TestController : ApiControllerBase<TestController>, ITestController
    {


        public TestController
            (
                ILoggerFactory loggerFactory,
                IConfiguration config

            )
            : base(loggerFactory, config)
        {


        }

        [Route(nameof(TestAdd1))]
        [HttpPost]
        public async Task<ApiResponse<DomainModel1>> TestAdd1([FromBody] Model1AddRequest request)
        {
            request.Request.Id = TestIdProvider.NextId<DomainModel1>();
            return await GetResult(async () => await Task.FromResult(request.Request));
        }

        [Route(nameof(TestAdd2))]
        [HttpPost]
        public async Task<ApiResponse<DomainModel2>> TestAdd2([FromBody] Model2AddRequest request)
        {
            request.Request.Id = TestIdProvider.NextId<DomainModel2>();
            return await GetResult(async () => await Task.FromResult(request.Request));
        }

        [Route(nameof(TestDelete))]
        [HttpGet]
        public async Task<ApiResponse<bool>> TestDelete(int id)
        {
            return await GetResult(async () => await Task.FromResult(true));
        }

        [Route(nameof(TestGetById1))]
        [HttpGet]
        public async Task<ApiResponse<DomainModel1>> TestGetById1(int id)
        {
            return await GetResult(async () => await Task.FromResult(new DomainModel1 { Id=id} ));
        }

        [Route(nameof(TestGetById2))]
        [HttpGet]
        public async Task<ApiResponse<DomainModel2>> TestGetById2(int id)
        {
            return await GetResult(async () => await Task.FromResult(new DomainModel2 { Id = id }));
        }

        [Route(nameof(TestUpdate1))]
        [HttpPut]
        public async Task<ApiResponse<DomainModel1>> TestUpdate1([FromBody] Model1UpdateRequest request)
        {
            return await GetResult(async () => await Task.FromResult(request.Request));
        }

        [Route(nameof(TestUpdate2))]
        [HttpPut]
        public async Task<ApiResponse<DomainModel2>> TestUpdate2([FromBody] Model2UpdateRequest request)
        {
            return await GetResult(async () => await Task.FromResult(request.Request));
        }

    }
}
