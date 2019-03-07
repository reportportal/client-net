﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ReportPortal.Client.Api.Launch.Model;
using ReportPortal.Client.Api.Launch.Request;
using ReportPortal.Client.Api.TestItem.Model;
using ReportPortal.Client.Api.TestItem.Request;
using ReportPortal.Client.Common.Model.Filtering;
using ReportPortal.Client.Common.Model.Paging;
using Xunit;

namespace ReportPortal.Client.Tests.LaunchItem
{
    public class LaunchItemFixture : BaseFixture
    {
        [Fact]
        public async Task GetInvalidLaunch()
        {
            await Assert.ThrowsAsync<HttpRequestException>(async () => await Service.Launch.GetLaunchAsync("invalid_id"));
        }

        [Fact]
        public async Task GetLaunches()
        {
            var container = await Service.Launch.GetLaunchesAsync();
            var launches = container.Collection.ToList();
            Assert.True(launches.Any());
        }

        [Fact]
        public async Task GetDebugLaunches()
        {
            var launches = await Service.Launch.GetLaunchesAsync(debug: true);
            foreach (var launch in launches.Collection)
            {
                Assert.Equal(LaunchMode.Debug, launch.Mode);
            }
        }

        [Fact]
        public async Task GetTheFirst10Launches()
        {
            var launches = await Service.Launch.GetLaunchesAsync(new FilterOption
            {
                Paging = new Page(1, 10)
            });
            Assert.Equal(10, launches.Collection.Count());
        }

        [Fact]
        public async Task GetLaunchesFilteredByName()
        {
            var launches = await Service.Launch.GetLaunchesAsync(new FilterOption
            {
                Paging = new Page(1, 10),
                FilterConditions = new List<FilterCondition> { new FilterCondition(FilterOperation.Contains, "name", "test") }
            });
            Assert.True(launches.Collection.Any());
            foreach (var launch in launches.Collection)
            {
                Assert.Contains("test", launch.Name.ToLower());
            }
        }

        [Fact]
        public async Task GetLaunchesSortedByAscendingDate()
        {
            var launches = await Service.Launch.GetLaunchesAsync(new FilterOption
            {
                Paging = new Page(1, 10),
                Sorting = new Sorting(new List<string> { "start_time" }, SortDirection.Ascending)
            });

            Assert.True(launches.Collection.Any());

            Assert.Equal(launches.Collection.Select(l => l.StartTime).OrderBy(st => st), launches.Collection.Select(l => l.StartTime));
        }

        [Fact]
        public async Task GetLaunchesSortedByDescendingDate()
        {
            var launches = await Service.Launch.GetLaunchesAsync(new FilterOption
            {
                Paging = new Page(1, 10),
                Sorting = new Sorting(new List<string> { "start_time" }, SortDirection.Descending)
            });

            Assert.True(launches.Collection.Any());

            Assert.Equal(launches.Collection.Select(l => l.StartTime).OrderByDescending(st => st), launches.Collection.Select(l => l.StartTime));
        }

        [Fact]
        public async Task StartFinishDeleteLaunch()
        {
            var startLaunchRequest = new StartLaunchRequest
            {
                Name = "StartFinishDeleteLaunch",
                StartTime = DateTime.UtcNow
            };

            var launch = await Service.Launch.StartLaunchAsync(startLaunchRequest);
            Assert.NotNull(launch.Id);

            var finishLaunchRequest = new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            var message = await Service.Launch.FinishLaunchAsync(launch.Id, finishLaunchRequest);
            Assert.Contains("successfully", message.Info);

            var gotLaunch = await Service.Launch.GetLaunchAsync(launch.Id);
            Assert.Equal("StartFinishDeleteLaunch", gotLaunch.Name);
            Assert.Equal(startLaunchRequest.StartTime, gotLaunch.StartTime);
            Assert.Equal(finishLaunchRequest.EndTime, gotLaunch.EndTime);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task StartUpdateFinishDeleteLaunch()
        {
            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartFinishDeleteLaunch",
                StartTime = DateTime.UtcNow
            });

            var updateMessage = await Service.Launch.UpdateLaunchAsync(launch.Id, new UpdateLaunchRequest()
            {
                Description = launch.Description,
                Mode = launch.Mode,
                Tags = launch.Tags
            });

            Assert.NotNull(launch.Id);
            Assert.Contains("successfully updated", updateMessage.Info);
            var message = await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);

            var gotLaunch = await Service.Launch.GetLaunchAsync(launch.Id);
            Assert.Equal("StartFinishDeleteLaunch", gotLaunch.Name);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task StartFinishDeleteFullLaunch()
        {
            var now = DateTime.UtcNow;
            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartFinishDeleteFullLaunch",
                Description = "Desc",
                StartTime = now,
                Tags = new List<string> { "tag1", "tag2", "tag3" },
            });
            Assert.NotNull(launch.Id);
            var getLaunch = await Service.Launch.GetLaunchAsync(launch.Id);
            Assert.Equal("StartFinishDeleteFullLaunch", getLaunch.Name);
            Assert.Equal("Desc", getLaunch.Description);
            Assert.Equal(now.ToString(), getLaunch.StartTime.ToString());
            Assert.Equal(new List<string> { "tag1", "tag2", "tag3" }, getLaunch.Tags);
            var message = await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);
            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task StartFinishDeleteMergedLaunch()
        {
            var launch1 = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartFinishDeleteLaunch",
                StartTime = DateTime.UtcNow
            });
            Assert.NotNull(launch1.Id);
            var message = await Service.Launch.FinishLaunchAsync(launch1.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);

            var launch2 = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartFinishDeleteLaunch2",
                StartTime = DateTime.UtcNow
            });
            Assert.NotNull(launch2.Id);
            message = await Service.Launch.FinishLaunchAsync(launch2.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);

            var mergeRequest = new MergeLaunchesRequest
            {
                Name = "MergedLaunch",
                Launches = new List<string> { launch1.Id, launch2.Id },
                MergeType = "BASIC",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow
            };

            var mergedLaunch = await Service.Launch.MergeLaunchesAsync(mergeRequest);
            Assert.Equal(mergeRequest.StartTime, mergedLaunch.StartTime);
            Assert.Equal(mergeRequest.EndTime, mergedLaunch.EndTime);

            var delMessage = await Service.Launch.DeleteLaunchAsync(mergedLaunch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task StartFinishAnalyzeDeleteLaunch()
        {
            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartFinishDeleteLaunch",
                StartTime = DateTime.UtcNow,
                Mode = LaunchMode.Default
            });
            Assert.NotNull(launch.Id);
            var message = await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);

            var gotLaunch = await Service.Launch.GetLaunchAsync(launch.Id);
            Assert.Equal("StartFinishDeleteLaunch", gotLaunch.Name);

            var analyzeMessage = await Service.Launch.AnalyzeLaunchAsync(new AnalyzeLaunchRequest
            {
                LaunchId = launch.Id,
                AnalyzerMode = AnalyzerMode.LaunchName,
                AnalyzerItemsMode = new List<AnalyzerItemsMode> { AnalyzerItemsMode.ToInvestigate }
            });
            Assert.Contains("started", analyzeMessage.Info);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task TrimLaunchName()
        {
            var namePrefix = "TrimLaunch";
            var launchName = namePrefix + new string('_', 256 - namePrefix.Length + 1);

            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = launchName,
                StartTime = DateTime.UtcNow
            });
            Assert.NotNull(launch.Id);
            var message = await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            });
            Assert.Contains("successfully", message.Info);

            var gotLaunch = await Service.Launch.GetLaunchAsync(launch.Id);
            Assert.Equal(launchName.Substring(0, 256), gotLaunch.Name);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task StartForceFinishIncompleteLaunch()
        {
            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartForceFinishIncompleteLaunch",
                StartTime = DateTime.UtcNow,
                Mode = LaunchMode.Default
            });

            var test = await Service.TestItem.StartTestItemAsync(new StartTestItemRequest
            {
                LaunchId = launch.Id,
                Name = "Test1",
                StartTime = DateTime.UtcNow,
                Type = TestItemType.Test
            });
            Assert.NotNull(test.Id);

            await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            }, true);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }

        [Fact]
        public async Task GetInProgressLaunch()
        {
            var launch = await Service.Launch.StartLaunchAsync(new StartLaunchRequest
            {
                Name = "StartForceFinishIncompleteLaunch",
                StartTime = DateTime.UtcNow,
                Mode = LaunchMode.Default
            });

            var getLaunch = await Service.Launch.GetLaunchAsync(launch.Id);

            Assert.NotEqual(default(DateTime), getLaunch.StartTime);
            Assert.Null(getLaunch.EndTime);

            await Service.Launch.FinishLaunchAsync(launch.Id, new FinishLaunchRequest
            {
                EndTime = DateTime.UtcNow
            }, true);

            var delMessage = await Service.Launch.DeleteLaunchAsync(launch.Id);
            Assert.Contains("successfully", delMessage.Info);
        }
    }
}
