using System;
using System.Diagnostics;
using System.Reflection;

namespace WebApiClient
{
    public class ApiProxyTest
    {
        public async Task TestProxyGet()
        {
            var proxy = new ApiProxy<TestController>();
            var add1Request = new Model1AddRequest { Request = new DomainModel1 { Name = "Test" } };
            var add1 = proxy.Controller.TestAdd1(add1Request);
            var result = await  proxy.Controller.TestGetById1(1);

        }
    }
    public interface IApiProxy<out T> where T: class
    {
        T Controller { get; }
    }
    public abstract class ApiProxy
    {
        public object Object => OnGetObject();
        protected abstract object OnGetObject();
    }
    public class ApiProxy<T>: ApiProxy, IApiProxy<T> where T: class
    {
        private T instance;
        public new virtual T Controller => (T)base.Object;

        protected override object OnGetObject()
        {
            if (instance == null)
            {
                InitializeInstance();
            }

            return instance;
        }

        [DebuggerStepThrough]
        private void InitializeInstance()
        {
            //instance = (T)ProxyFactory.Instance.CreateProxy(typeof(T), this, array, constructorArguments);
        }
    }

    internal abstract class ApiProxyFactory
    {

    }
}
