using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using ReactiveUI;
using Xunit;

public class RepositoryPublishViewModelTests
{
    public class TheRepositoryHostsProperty
    {
        [Theory]
        [InlineData(true, true, 2)]
        [InlineData(true, false, 1)]
        [InlineData(false, true, 1)]
        public void IncludesLoggedInRepositories(bool gitHubLoggedIn, bool enterpriseLoggedIn, int expectedCount)
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(gitHubLoggedIn);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var enterpriseHost = Substitute.For<IRepositoryHost>();
            enterpriseHost.IsLoggedIn.Returns(enterpriseLoggedIn);
            enterpriseHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            enterpriseHost.Title.Returns("ghe.io");
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            hosts.EnterpriseHost.Returns(enterpriseHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            var repositoryHosts = vm.RepositoryHosts;

            Assert.Equal(expectedCount, repositoryHosts.Count);
        }
    }

    public class TheSelectedHostProperty
    {
        [Fact]
        public void DefaultsToGitHub()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            Assert.Same(gitHubHost, vm.SelectedHost);
        }
    }

    public class TheAccountsProperty
    {
        [Fact]
        public void IsPopulatedByTheAccountsForTheSelectedHost()
        {
            var gitHubAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            var enterpriseAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>() };
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(gitHubAccounts));
            var enterpriseHost = Substitute.For<IRepositoryHost>();
            enterpriseHost.IsLoggedIn.Returns(true);
            enterpriseHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(enterpriseAccounts));
            enterpriseHost.Title.Returns("ghe.io");
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            hosts.EnterpriseHost.Returns(enterpriseHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            Assert.Equal(2, vm.Accounts.Count);
            Assert.Same(gitHubAccounts[0], vm.SelectedAccount);

            vm.SelectedHost = enterpriseHost;

            Assert.Equal(1, vm.Accounts.Count);
            Assert.Same(enterpriseAccounts[0], vm.SelectedAccount);
        }
    }

    public class TheSafeRepositoryNameProperty
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());
            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.Null(vm.SafeRepositoryName);
        }
    }

    public class TheRepositoryNameValidatorProperty
    {
        [Fact]
        public void IsFalseWhenRepoNameEmpty()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());
            vm.RepositoryName = "repo";

            Assert.True(vm.PublishRepository.CanExecute(null));
            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsTrueWhenRepositoryNameIsValid()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty
    {
        [Fact]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.GetAccounts(Args.AvatarProvider).Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = new RepositoryPublishViewModel(
                hosts,
                Substitute.For<IAvatarProvider>(),
                Substitute.For<IRepositoryPublishService>());

            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }
    }
}
