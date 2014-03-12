// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Test.Interception
{
    using System;
    using System.Threading.Tasks;

    public class TestService : ITestService
    {
        #region Fields
        private string _name;
        #endregion

        #region Constructors
        public TestService()
        {
            _name = "testValue";
        }
        #endregion

        #region ITestService Members
        public bool WasExecuted { get; set; }

        public virtual string Name
        {
            get
            {
                WasExecuted = true;
                return _name;
            }

            set
            {
                _name = value;
                WasExecuted = true;
            }
        }

        public string Description { get; set; }

        public void Fail()
        {
            WasExecuted = true;
            throw new InvalidOperationException();
        }

        public Task FailAsync()
        {
            return Task.Factory.StartNew(Fail);
        }

        public int Return()
        {
            WasExecuted = true;
            return 1;
        }

        public Task<int> ReturnAsync()
        {
            return Task.Factory.StartNew(() => Return());
        }

        public void TaggedPerform()
        {
            WasExecuted = true;
        }

        public Task TaggedPerformAsync()
        {
            return Task.Factory.StartNew(TaggedPerform);
        }

        public virtual void Perform()
        {
            WasExecuted = true;
        }

        public Task PerformAsync()
        {
            return Task.Factory.StartNew(Perform);
        }

        public void Perform(string value)
        {
            WasExecuted = true;
        }

        public Task PerformAsync(string value)
        {
            return PerformAsync();
        }

        public void Perform(int a)
        {
            WasExecuted = true;
        }

        public Task PerformAsync(int value)
        {
            return PerformAsync();
        }

        public T Perform<T>(T instance)
        {
            WasExecuted = true;
            return instance;
        }

        public Task<T> PerformAsync<T>(T instance)
        {
            return Task.Factory.StartNew(() => Perform(instance));
        }
        #endregion
    }
}