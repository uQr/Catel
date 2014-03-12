// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodInterceptionTests.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Test.Interception
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Catel.IoC;
    using Catel.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodInterceptionTests
    {
        #region Fields
        private IServiceLocator _serviceLocator;
        #endregion

        #region Methods
        [TestInitialize]
        public void Initialization()
        {
            _serviceLocator = ServiceLocator.Default;
        }

        [TestMethod]
        public void ShouldCallbackOnBefore()
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform())
                           .OnBefore(invocation => Assert.IsFalse(((ITestService) invocation.Target).WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

            Assert.IsTrue(resolvedTestService.WasExecuted);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldCallbackOnBeforeForAsyncMethod()
#else
        public async Task ShouldCallbackOnBeforeForAsyncMethod()
#endif
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync())
                           .OnBefore(invocation => Assert.IsFalse(((ITestService)invocation.Target).WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync().ContinueWith(task => Assert.IsTrue(resolvedTestService.WasExecuted));
#else
            await resolvedTestService.PerformAsync();
            Assert.IsTrue(resolvedTestService.WasExecuted);
#endif
        }

        [TestMethod]
        public void ShouldCallbackOnAfter()
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform())
                           .OnAfter(invocation => Assert.IsTrue(((ITestService) invocation.Target).WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

            Assert.IsTrue(resolvedTestService.WasExecuted);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldCallbackOnAfterForAsyncMethod()
#else
        public async Task ShouldCallbackOnAfterForAsyncMethod()
#endif
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync())
                           .OnAfter(invocation => Assert.IsTrue(((ITestService)invocation.Target).WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync().ContinueWith(_ => Assert.IsTrue(resolvedTestService.WasExecuted));
#else
            await resolvedTestService.PerformAsync();
            Assert.IsTrue(resolvedTestService.WasExecuted);
#endif
        }

        [TestMethod]
        public void ShouldCallbackOnCatchAndOnFinally()
        {
            var index = 0;

            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Fail())
                           .OnCatch(exception => exception.GetType().IsAssignableFromEx(typeof (InvalidOperationException)))
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Fail();

            Assert.AreEqual(1, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldCallbackOnCatchAndOnFinallyForAsyncMethod()
#else
        public async Task ShouldCallbackOnCatchAndOnFinallyForAsyncMethod()
#endif
        {
            var index = 0;

            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.FailAsync())
                           .OnCatch(exception => exception.GetType().IsAssignableFromEx(typeof(InvalidOperationException)))
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.FailAsync().ContinueWith(_ => Assert.AreEqual(1, index));
#else
            await resolvedTestService.FailAsync();
            Assert.AreEqual(1, index);
#endif
        }

        [TestMethod]
        public void ShouldCallbackOnBeforeOnFinallyAndOnAfter()
        {
            var index = 0;

            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform())
                           .OnBefore(invocation =>
                               {
                                   index++;
                                   Assert.IsFalse(((ITestService) invocation.Target).WasExecuted);
                               })
                           .OnFinally(() => index++)
                           .OnAfter(invocation =>
                               {
                                   index++;
                                   Assert.IsTrue(((ITestService) invocation.Target).WasExecuted);
                               });

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

            Assert.AreEqual(3, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldCallbackOnBeforeOnFinallyAndOnAfterForAsyncMethod()
#else
        public async Task ShouldCallbackOnBeforeOnFinallyAndOnAfterForAsyncMethod()
#endif
        {
            var index = 0;

            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync())
                           .OnBefore(invocation =>
                           {
                               index++;
                               Assert.IsFalse(((ITestService)invocation.Target).WasExecuted);
                           })
                           .OnFinally(() => index++)
                           .OnAfter(invocation =>
                           {
                               index++;
                               Assert.IsTrue(((ITestService)invocation.Target).WasExecuted);
                           });

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync().ContinueWith(_ => Assert.AreEqual(3, index));
#else
            await resolvedTestService.PerformAsync();
            Assert.AreEqual(3, index);
#endif
        }

        [TestMethod]
        public void ShouldCallbackOnReturnAndReplaceReturnValue()
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Return())
                           .OnReturn((invocation, returnedValue) => returnedValue.Equals(1) ? -1 : -2);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

            Assert.AreEqual(-1, resolvedTestService.Return());
            Assert.IsTrue(resolvedTestService.WasExecuted);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldCallbackOnReturnAndReplaceReturnValueForAsyncMethod()
#else
        public async Task ShouldCallbackOnReturnAndReplaceReturnValueForAsyncMethod()
#endif
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.ReturnAsync())
                           .OnReturn((invocation, returnedValue) => ((Task<int>) returnedValue).Result.Equals(1) ? -1 : -2);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

#if NET40 || SL5 || PCL
            resolvedTestService.ReturnAsync().ContinueWith(task =>
            {
                Assert.AreEqual(-1, task.Result);
                Assert.IsTrue(resolvedTestService.WasExecuted);
            });
#else
            var result = await resolvedTestService.ReturnAsync();
            Assert.AreEqual(-1, result);
            Assert.IsTrue(resolvedTestService.WasExecuted);
#endif
        }

        [TestMethod]
        public void CanCallbackOnFinallyIfMethodThrows()
        {
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Fail())
                           .OnFinally(invocation => Assert.IsTrue(((ITestService) invocation.Target).WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            ExceptionTester.CallMethodAndExpectException<TargetInvocationException>(resolvedTestService.Fail);
            Assert.IsTrue(resolvedTestService.WasExecuted);
        }

//        [TestMethod]
//#if NET40 || SL5 || PCL
//        public void CanCallbackOnFinallyIfMethodThrowsForAsyncMethod()
//#else
//        public async void CanCallbackOnFinallyIfMethodThrowsForAsyncMethod()
//#endif
//        {
//            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
//                           .InterceptMethod(service => service.FailAsync())
//                           .OnFinally(invocation => Assert.IsTrue(((ITestService)invocation.Target).WasExecuted));

//            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

//#if NET40 || SL5 || PCL
//            ExceptionTester.CallMethodAndExpectException<TargetInvocationException>(() => resolvedTestService.FailAsync().Wait());
//#else
//#endif
//            Assert.IsTrue(resolvedTestService.WasExecuted);

//        }

        [TestMethod]
        public void ShouldInterceptAllMembersExceptWhichTagged()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptAllMembers()
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            var name = resolvedTestService.Name;
            resolvedTestService.Description = "anyValue";
            var description = resolvedTestService.Description;
            resolvedTestService.Name = name;
            resolvedTestService.TaggedPerform();
            var result = resolvedTestService.Perform<string>(description);

            Assert.IsNotNull(result);
            Assert.AreEqual("anyValue", result);
            Assert.AreEqual(5, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptAllMembersExceptWhichTaggedIncludeAsyncMethods()
#else
        public async Task ShouldInterceptAllMembersExceptWhichTaggedIncludeAsyncMethods()
#endif
        {
            var index = 0;
            var result = string.Empty;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptAllMembers()
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            var name = resolvedTestService.Name;
            resolvedTestService.Description = "anyValue";
            var description = resolvedTestService.Description;
            resolvedTestService.Name = name;
#if NET40 || SL5 || PCL
            resolvedTestService.TaggedPerformAsync().Wait();
            resolvedTestService.PerformAsync<string>(description).ContinueWith(task => { result = task.Result; }).Wait();
#else
            await resolvedTestService.TaggedPerformAsync();
            result = await resolvedTestService.PerformAsync<string>(description);
#endif
            Assert.IsNotNull(result);
            Assert.AreEqual("anyValue", result);
            Assert.AreEqual(5, index);
        }

        [TestMethod]
        public void ShouldInterceptAllMethodsGettersAndSetters()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptAll()
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            var name = resolvedTestService.Name;
            resolvedTestService.Description = "anyValue";
            var description = resolvedTestService.Description;
            resolvedTestService.Name = name;
            var result = resolvedTestService.Perform<string>(description);

            Assert.IsNotNull(result);
            Assert.AreEqual("anyValue", result);
            Assert.AreEqual(5, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptAllAsyncMethodsGettersAndSetters()
#else
        public async Task ShouldInterceptAllAsyncMethodsGettersAndSetters()
#endif
        {
            var index = 0;
            var result = string.Empty;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptAll()
                           .OnFinally(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            var name = resolvedTestService.Name;
            resolvedTestService.Description = "anyValue";
            var description = resolvedTestService.Description;
            resolvedTestService.Name = name;
#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync<string>(description)
                               .ContinueWith(task => { result = task.Result; })
                                .ContinueWith(_ =>
                                {
                                    Assert.IsNotNull(result);
                                    Assert.AreEqual("anyValue", result);
                                    Assert.AreEqual(5, index);
                                });
#else
            result = await resolvedTestService.PerformAsync<string>(description);
            Assert.IsNotNull(result);
            Assert.AreEqual("anyValue", result);
            Assert.AreEqual(5, index);
#endif
        }

        [TestMethod]
        public void ShouldInterceptGenericMethods()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform<int>(It.IsAny<int>()))
                           .OnBefore(invocation =>
                               {
                                   Assert.IsFalse(((ITestService) invocation.Target).WasExecuted);
                                   index++;
                               })
                           .And()
                           .InterceptMethod(service => service.Perform<string>(It.IsAny<string>()))
                           .OnAfter(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform<int>(1); // intercepted
            resolvedTestService.Perform<string>(string.Empty); // intercepted
            resolvedTestService.Perform(2d); // not intercepted
            resolvedTestService.Perform(It.IsAny<object>()); // not intercepted

            Assert.IsTrue(resolvedTestService.WasExecuted);
            Assert.AreEqual(2, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptGenericAsyncMethods()
#else
        public async Task ShouldInterceptGenericAsyncMethods()
#endif
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync<int>(It.IsAny<int>()))
                           .OnBefore(invocation =>
                           {
                               Assert.IsFalse(((ITestService)invocation.Target).WasExecuted);
                               index++;
                           })
                           .And()
                           .InterceptMethod(service => service.PerformAsync<string>(It.IsAny<string>()))
                           .OnAfter(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync<int>(1).Wait(); // intercepted
            resolvedTestService.PerformAsync<string>(string.Empty).Wait(); // intercepted
            resolvedTestService.PerformAsync(2d).Wait(); // not intercepted
            resolvedTestService.PerformAsync(It.IsAny<object>()).Wait(); // not intercepted
#else
            await resolvedTestService.PerformAsync<int>(1); // intercepted
            await resolvedTestService.PerformAsync<string>(string.Empty); // intercepted
            await resolvedTestService.PerformAsync(2d); // not intercepted
            await resolvedTestService.PerformAsync(It.IsAny<object>()); // not intercepted
#endif

            Assert.IsTrue(resolvedTestService.WasExecuted);
            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void ShouldInterceptMultipleMembersUsingFluentInstruction()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform())
                           .OnBefore(() => Assert.AreEqual(0, index))
                           .OnAfter(() => index++)
                           .And()
                           .InterceptGetter(service => service.Name)
                           .OnBefore(() => Assert.AreEqual(1, index))
                           .OnAfter(() => index++)
                           .And()
                           .InterceptSetter(service => service.Description)
                           .OnBefore(() => Assert.AreEqual(2, index))
                           .OnAfter(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            // intercepted
            resolvedTestService.Perform();
            var name = resolvedTestService.Name;
            resolvedTestService.Description = string.Empty;

            // not intercpted
            resolvedTestService.Return();
            resolvedTestService.Name = name;
            var description = resolvedTestService.Description;

            Assert.AreEqual(string.Empty, description);
            Assert.AreEqual(3, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptMultipleMembersUsingFluentInstructionIncludeAsyncMethods()
#else
        public async Task ShouldInterceptMultipleMembersUsingFluentInstructionIncludeAsyncMethods()
#endif
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync())
                           .OnBefore(() => Assert.AreEqual(0, index))
                           .OnAfter(() => index++)
                           .And()
                           .InterceptGetter(service => service.Name)
                           .OnBefore(() => Assert.AreEqual(1, index))
                           .OnAfter(() => index++)
                           .And()
                           .InterceptSetter(service => service.Description)
                           .OnBefore(() => Assert.AreEqual(2, index))
                           .OnAfter(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            // intercepted
            resolvedTestService.PerformAsync().Wait();
            var name = resolvedTestService.Name;
            resolvedTestService.Description = string.Empty;

            // not intercpted
            resolvedTestService.ReturnAsync().Wait();
            resolvedTestService.Name = name;
            var description = resolvedTestService.Description;
#else
            // intercepted
            await resolvedTestService.PerformAsync();
            var name = resolvedTestService.Name;
            resolvedTestService.Description = string.Empty;

            // not intercpted
            await resolvedTestService.ReturnAsync();
            resolvedTestService.Name = name;
            var description = resolvedTestService.Description;
#endif

            Assert.AreEqual(string.Empty, description);
            Assert.AreEqual(3, index);
        }

        [TestMethod]
        public void ShouldInterceptOverloadedMethods()
        {
            var value = false;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.Perform(It.IsAny<string>()))
                           .OnBefore(() => value = true);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform(-1); // not intercepted
            Assert.IsFalse(value);
            resolvedTestService.Perform(string.Empty); // intercepted
            Assert.IsTrue(value);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptOverloadedAsyncMethods()
#else
        public async Task ShouldInterceptOverloadedAsyncMethods()
#endif
        {
            var value = false;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethod(service => service.PerformAsync(It.IsAny<string>()))
                           .OnBefore(() => value = true);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync(-1).Wait(); // not intercepted
            Assert.IsFalse(value);
            resolvedTestService.PerformAsync(string.Empty).Wait(); // intercepted
            Assert.IsTrue(value);
#else
            await resolvedTestService.PerformAsync(-1); // not intercepted
            Assert.IsFalse(value);
            await resolvedTestService.PerformAsync(string.Empty); // intercepted
            Assert.IsTrue(value);
#endif
        }

        [Ignore]
        [TestMethod]
        public void ShouldInterceptMethodUsingPredicate()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptWhere(invocation => invocation.Name == "Perform")
                           .OnBefore(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            // intercepted
            resolvedTestService.Perform();
            resolvedTestService.Perform<int>(-1);
            resolvedTestService.Perform(-1);
            resolvedTestService.Perform(string.Empty);

            // not intercepted
            resolvedTestService.Name = string.Empty;
            resolvedTestService.Return();

            Assert.AreEqual(4, index);
        }

        [TestMethod]
        public void ShouldInterceptManyMethodsWithOneCallback()
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethods(
                               service => service.Return(),
                               service => service.Perform<int>(It.IsAny<int>()),
                               service => service.Perform())
                           .OnBefore(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            // intercepted
            resolvedTestService.Perform();
            resolvedTestService.Return();
            resolvedTestService.Perform<int>(-1);

            // not intercepted
            resolvedTestService.Name = string.Empty;
            resolvedTestService.Perform(string.Empty);

            Assert.AreEqual(3, index);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInterceptManyAsyncMethodsWithOneCallback()
#else
        public async Task ShouldInterceptManyAsyncMethodsWithOneCallback()
#endif
        {
            var index = 0;
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>()
                           .InterceptMethods(
                               service => service.ReturnAsync(),
                               service => service.PerformAsync<int>(It.IsAny<int>()),
                               service => service.PerformAsync())
                           .OnBefore(() => index++);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            // intercepted
            resolvedTestService.PerformAsync().Wait();
            resolvedTestService.ReturnAsync().Wait();
            resolvedTestService.PerformAsync<int>(-1).Wait();

            // not intercepted
            resolvedTestService.Name = string.Empty;
            resolvedTestService.PerformAsync(string.Empty).Wait();
#else
            // intercepted
            await resolvedTestService.PerformAsync();
            resolvedTestService.ReturnAsync().Wait();
            await resolvedTestService.PerformAsync<int>(-1);

            // not intercepted
            resolvedTestService.Name = string.Empty;
            await resolvedTestService.PerformAsync(string.Empty);
#endif

            Assert.AreEqual(3, index);
        }

        [TestMethod]
        public void ShouldUseTheProvidedTargetInstance()
        {
            var testService = new TestService{Name = "providedInstance"};
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.Perform())
                           .OnBefore(invocation => Assert.AreEqual(testService.Name, ((ITestService) invocation.Target).Name));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldUseTheProvidedTargetInstanceToAssertAsyncMethod()
#else
        public async Task ShouldUseTheProvidedTargetInstanceToAssertAsyncMethod()
#endif
        {
            var testService = new TestService { Name = "providedInstance" };
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.PerformAsync())
                           .OnBefore(invocation => Assert.AreEqual(testService.Name, ((ITestService)invocation.Target).Name));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync();
#else
            await resolvedTestService.PerformAsync();
#endif
        }

        [TestMethod]
        public void ShouldInferTargetMethodIfATargetObjectExists()
        {
            var testService = new TestService();
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.Perform())
                           .OnBefore(invocation => Assert.IsFalse(testService.WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            resolvedTestService.Perform();

            Assert.IsTrue(testService.WasExecuted);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldInferTargetAsyncMethodIfATargetObjectExists()
#else
        public async Task ShouldInferTargetAsyncMethodIfATargetObjectExists()
#endif
        {
            var testService = new TestService();
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.PerformAsync())
                           .OnBefore(invocation => Assert.IsFalse(testService.WasExecuted));

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.PerformAsync().ContinueWith(_ => Assert.IsTrue(testService.WasExecuted));
#else
            await resolvedTestService.PerformAsync();
            Assert.IsTrue(testService.WasExecuted);
#endif
        }

        [TestMethod]
        public void ShouldSkipTarget()
        {
            var testService = new TestService();
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.Return())
                           .OnInvoke(invocation => -2);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

            Assert.AreEqual(-2, resolvedTestService.Return());

            Assert.IsFalse(testService.WasExecuted);
        }

        [TestMethod]
#if NET40 || SL5 || PCL
        public void ShouldSkipTargetOnCallingAsyncMethods()
#else
        public async Task ShouldSkipTargetOnCallingAsyncMethods()
#endif
        {
            var testService = new TestService();
            _serviceLocator.ConfigureInterceptionForType<ITestService, TestService>(targetInstanceToUse: testService)
                           .Intercept(service => service.ReturnAsync())
                           .OnInvoke(invocation => -2);

            var resolvedTestService = _serviceLocator.ResolveType<ITestService>();

#if NET40 || SL5 || PCL
            resolvedTestService.ReturnAsync().ContinueWith(task =>
            {
                Assert.AreEqual(-2, task.Result);
                Assert.IsFalse(testService.WasExecuted);
            });
#else
            var result = await resolvedTestService.ReturnAsync();
            Assert.AreEqual(-2, result);
            Assert.IsFalse(testService.WasExecuted);
#endif
        }
        #endregion
    }
}