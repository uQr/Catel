// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITestService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Test.Interception
{
    using System.Threading.Tasks;
    using Catel.Interception;

    public interface ITestService
    {
        #region Properties
        string Name { get; set; }
        string Description { get; set; }
        bool WasExecuted { get; set; }
        #endregion

        #region Methods
        void Perform();

        Task PerformAsync();

        void Perform(string value);

        Task PerformAsync(string value);

        void Perform(int value);

        Task PerformAsync(int value);

        T Perform<T>(T instance);

        Task<T> PerformAsync<T>(T instance);

        [DoNotIntercept]
        void TaggedPerform();

        [DoNotIntercept]
        Task TaggedPerformAsync();

        void Fail();

        Task FailAsync();

        int Return();

        Task<int> ReturnAsync();
        #endregion
    }
}