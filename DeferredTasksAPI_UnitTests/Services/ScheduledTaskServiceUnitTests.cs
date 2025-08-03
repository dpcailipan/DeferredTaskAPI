using DeferredTaskAPI.Entities;
using DeferredTaskAPI.Models;
using DeferredTaskAPI.Repositories.Interfaces;
using DeferredTaskAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DeferredTasksAPIUnitTests.Services
{
    [TestClass]
    public class ScheduledTaskServiceUnitTests
    {
        private ScheduledTaskService _sut;
        private Mock<ILogger<ScheduledTaskService>> _mockLogger;
        private Mock<IScheduledTasksRepository> _mockRepository;
        private ScheduledTask _existingScheduledTask;

        [TestInitialize]
        public void InitializeTest()
        {
            _mockLogger = new Mock<ILogger<ScheduledTaskService>>();
            _mockRepository = new Mock<IScheduledTasksRepository>();
            _existingScheduledTask = new ScheduledTask
            {
                Id = Guid.NewGuid(),
                Title = "Existing Title",
                Description = "Existing Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _ = _mockRepository
                .Setup(m => m.Get(It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns((Guid id, bool trackChanges) =>
                {
                    if (id == _existingScheduledTask.Id)
                    {
                        return _existingScheduledTask;
                    }

                    return null;
                });
            _ = _mockRepository
                .Setup(m => m.CreateAsync(It.IsAny<ScheduledTask>()))
                .Callback((ScheduledTask scheduledTask) =>
                {
                    scheduledTask.Id = Guid.NewGuid();
                });
            _ = _mockRepository
                .Setup(m => m.GetAll(It.IsAny<bool>()))
                .Returns(new List<ScheduledTask> { _existingScheduledTask });
            _ = _mockRepository
                .Setup(m => m.GetAllExecutable())
                .Returns(new List<ScheduledTask> { _existingScheduledTask });
            _ = _mockRepository
                .Setup(m => m.SaveChangesAsync())
                .Returns(() =>
                {
                    return Task.FromResult(_mockRepository.Invocations.Count);
                });

            _sut = new ScheduledTaskService(_mockLogger.Object, _mockRepository.Object);
        }

        #region CreateScheduledTaskAsync
        [TestMethod]
        public async Task CreateScheduledTaskAsync_HappyPath_ShouldReturnScheduledTask()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test Title",
                Description = "Test Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(1),
            };
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                IsSuccess = true,
                Value = new ScheduledTask
                {
                    Title = request.Title,
                    Description = request.Description,
                    ScheduledTime = request.ScheduledTime,
                    ExecutedAt = null,
                    IsExecuted = false,
                }
            };

            var actual = await _sut.CreateScheduledTaskAsync(request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Value.Id)
                        .Excluding(st => st.Value.CreatedAt)
                        .Excluding(st => st.Value.UpdatedAt));
            _mockRepository.Verify(
                m => m.CreateAsync(It.IsAny<ScheduledTask>()),
                Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task CreateScheduledTaskAsync_ErrorWhenSaving_ShouldReturnInternalServerError()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test Title",
                Description = "Test Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(1),
            };
            _ = _mockRepository
                .Setup(m => m.SaveChangesAsync())
                .Returns(() =>
                {
                    return Task.FromResult(0);
                });
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsFailed = true,
            };

            var actual = await _sut.CreateScheduledTaskAsync(request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(
                m => m.CreateAsync(It.IsAny<ScheduledTask>()),
                Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task CreateScheduledTaskAsync_InvalidRequest_ShouldReturnBadRequest()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test Title aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Description = "Test Description aaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ScheduledTime = DateTime.UtcNow.AddMinutes(-1),
            };
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsFailed = true,
            };

            var actual = await _sut.CreateScheduledTaskAsync(request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(3);
            _mockRepository.Verify(
                m => m.CreateAsync(It.IsAny<ScheduledTask>()),
                Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }
        #endregion

        #region GetScheduledTasks
        [TestMethod]
        public void GetScheduledTasks_HappyPath_ShouldReturnScheduledTasks()
        {
            var expected = new ApiResult<IEnumerable<ScheduledTask>>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Value = new List<ScheduledTask> { _existingScheduledTask }
            };

            var actual = _sut.GetScheduledTasks();

            _ = actual.Should()
                .BeEquivalentTo(expected);
            _mockRepository.Verify(
                m => m.GetAll(It.Is<bool>(p => !p)),
                Times.Once());
        }
        #endregion

        #region GetScheduledTask
        [TestMethod]
        public void GetScheduledTask_HappyPath_ShouldReturnScheduledTask()
        {
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Value = _existingScheduledTask
            };

            var actual = _sut.GetScheduledTask(_existingScheduledTask.Id);

            _ = actual.Should()
                .BeEquivalentTo(expected);
            _mockRepository.Verify(
                m => m.Get(It.Is<Guid>(p => p == _existingScheduledTask.Id), It.Is<bool>(p => !p)),
                Times.Once());
        }

        [TestMethod]
        public void GetScheduledTask_InvalidId_ShouldReturnNotFound()
        {
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                IsFailed = true,
            };

            var actual = _sut.GetScheduledTask(Guid.NewGuid());

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(
                m => m.Get(It.Is<Guid>(p => p == _existingScheduledTask.Id), It.Is<bool>(p => !p)),
                Times.Never());
        }
        #endregion

        #region UpdateScheduledTaskAsync
        [TestMethod]
        public async Task UpdateScheduledTaskAsync_HappyPath_ShouldReturnUpdatedScheduledTask()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test New Title",
                Description = "Test New Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(2),
            };
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Value = new ScheduledTask
                {
                    Id = _existingScheduledTask.Id,
                    Title = request.Title,
                    Description = request.Description,
                    ScheduledTime = request.ScheduledTime,
                    CreatedAt = _existingScheduledTask.CreatedAt,
                    ExecutedAt = null,
                    IsExecuted = false,
                }
            };

            var actual = await _sut.UpdateScheduledTaskAsync(_existingScheduledTask.Id,
                request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Value.UpdatedAt));
            _mockRepository.Verify(
                m => m.Update(It.Is<ScheduledTask>(p => p.Id == _existingScheduledTask.Id)),
                Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateScheduledTaskAsync_InvalidId_ShouldReturnNotFound()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test New Title",
                Description = "Test New Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(2),
            };
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                IsFailed = true,
            };

            var actual = await _sut.UpdateScheduledTaskAsync(Guid.NewGuid(), request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Update(It.IsAny<ScheduledTask>()), Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }

        [TestMethod]
        public async Task UpdateScheduledTaskAsync_ErrorWhenSaving_ShouldReturnInternalServerError()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test New Title",
                Description = "Test New Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(2),
            };
            _ = _mockRepository
                .Setup(m => m.SaveChangesAsync())
                .Returns(() =>
                {
                    return Task.FromResult(0);
                });
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsFailed = true,
            };

            var actual = await _sut.UpdateScheduledTaskAsync(_existingScheduledTask.Id,
                request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Update(It.IsAny<ScheduledTask>()), Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task UpdateScheduledTaskAsync_InvalidRequest_ShouldReturnBadRequest()
        {
            var request = new ScheduledTaskRequest
            {
                Title = "Test Title aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Description = "Test Description aaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                ScheduledTime = DateTime.UtcNow.AddMinutes(-1),
            };
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsFailed = true,
            };

            var actual = await _sut.UpdateScheduledTaskAsync(_existingScheduledTask.Id,
                request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(3);
            _mockRepository.Verify(m => m.Update(It.IsAny<ScheduledTask>()), Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }

        [TestMethod]
        public async Task UpdateScheduledTaskAsync_TaskAlreadyExecuted_ShouldReturnBadRequest()
        {
            _existingScheduledTask.IsExecuted = true;
            _existingScheduledTask.ExecutedAt = DateTime.UtcNow;
            var request = new ScheduledTaskRequest
            {
                Title = "Test New Title",
                Description = "Test New Description",
                ScheduledTime = DateTime.UtcNow.AddMinutes(2),
            };
            var expected = new ApiResult
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsFailed = true,
            };

            var actual = await _sut.UpdateScheduledTaskAsync(_existingScheduledTask.Id,
                request);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Update(It.IsAny<ScheduledTask>()), Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }
        #endregion

        #region DeleteScheduledTaskAsync
        [TestMethod]
        public async Task DeleteScheduledTaskAsync_HappyPath_ShouldDeleteTaskAndReturnSuccess()
        {
            var expected = new ApiResult<ScheduledTask>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
            };

            var actual = await _sut.DeleteScheduledTaskAsync(_existingScheduledTask.Id);

            _ = actual.Should()
                .BeEquivalentTo(expected);
            _mockRepository.Verify(m => m.Delete(It.IsAny<ScheduledTask>()), Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task DeleteScheduledTaskAsync_InvalidId_ShouldReturnNotFound()
        {
            var expected = new ApiResult
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                IsFailed = true,
            };

            var actual = await _sut.DeleteScheduledTaskAsync(Guid.NewGuid());

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Delete(It.IsAny<ScheduledTask>()), Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }

        [TestMethod]
        public async Task DeleteScheduledTaskAsync_ErrorWhenSaving_ShouldReturnInternalServerError()
        {
            _ = _mockRepository
                .Setup(m => m.SaveChangesAsync())
                .Returns(() =>
                {
                    return Task.FromResult(0);
                });
            var expected = new ApiResult
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsFailed = true,
            };

            var actual = await _sut.DeleteScheduledTaskAsync(_existingScheduledTask.Id);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Delete(It.IsAny<ScheduledTask>()), Times.Once());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task DeleteScheduledTaskAsync_InvalidRequest_ShouldReturnBadRequest()
        {
            _existingScheduledTask.IsExecuted = true;
            _existingScheduledTask.ExecutedAt = DateTime.UtcNow;
            var expected = new ApiResult
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsFailed = true,
            };

            var actual = await _sut.DeleteScheduledTaskAsync(_existingScheduledTask.Id);

            _ = actual.Should()
                .BeEquivalentTo(expected,
                    options => options
                        .Excluding(st => st.Errors));
            _ = actual.Errors?.Count().Should().Be(1);
            _mockRepository.Verify(m => m.Delete(It.IsAny<ScheduledTask>()), Times.Never());
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Never());
        }
        #endregion

        #region RunScheduledTaskAsync
        [TestMethod]
        public async Task RunScheduledTasksAsync_HappyPath_ShouldLogTaskAndTagAsExecuted()
        {
            await _sut.RunScheduledTasksAsync(CancellationToken.None);

            Assert.IsTrue(_existingScheduledTask.IsExecuted);
            _mockRepository.Verify(m => m.SaveChangesAsync(), Times.Once());
        }
        #endregion
    }
}
