using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DirSync.Executor;
using Xunit;

namespace DirSync.Test
{
    public class SyncExecutorTest : IDisposable
    {
        private readonly SyncExecutor _executor;
        private readonly Options _options;
        private readonly string _src;
        private readonly string _target;

        public SyncExecutorTest()
        {
            _src = TestHelper.GetRandomFolderPath();
            _target = TestHelper.GetRandomFolderPath();
            _options = new Options
            {
                SourceDir = _src,
                TargetDir = _target
            };
            _executor = new SyncExecutor(_options);
        }

        public void Dispose()
        {
            TestHelper.DeleteFolder(_src);
            TestHelper.DeleteFolder(_target);
            TestHelper.DeleteFolder(_options.SourceDir);
            TestHelper.DeleteFolder(_options.TargetDir);
        }

        [Fact]
        public async Task Test1_EmptySrc_NoTarget()
        {
            _options.TargetDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var executorResult = await _executor.ExecuteAsync();

            Assert.True(executorResult.Succeed);
            Assert.Equal(0, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(0, executorResult.SrcFileCount);
        }

        [Fact]
        public async Task Test2_Src_Has_File()
        {
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);
            var executorResult = await _executor.ExecuteAsync();

            Assert.True(executorResult.Succeed);
            Assert.Equal(9, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(9, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test3_Src_Has_File_And_Folder()
        {
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);
            var srcFolder1 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder1, 1, 10, new byte[2000]);
            var srcFolder2 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder2, 1, 10, new byte[3000]);

            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(27, executorResult.TargetAffectedFileCount);
            Assert.Equal(2, executorResult.SrcDirCount);
            Assert.Equal(27, executorResult.SrcFileCount);
            Assert.Equal(27, Directory.GetFiles(_target, "*", SearchOption.AllDirectories).Length);
            Assert.Equal(2, Directory.GetDirectories(_target, "*", SearchOption.AllDirectories).Length);
        }

        [Fact]
        public async Task Test4_Cleanup_On_No_Same_File()
        {
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_target, 10, 19, new byte[1000]);
            var srcFolder1 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder1, 1, 10, new byte[2000]);
            var srcFolder2 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder2, 1, 10, new byte[3000]);
            var targetFolder1 = TestHelper.GetFolderPath(_target);
            await TestHelper.CreateRandomFileAsync(targetFolder1, 10, 19, new byte[5000000]);

            _options.Cleanup = true;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(45, executorResult.TargetAffectedFileCount);
            Assert.Equal(2, executorResult.SrcDirCount);
            Assert.Equal(27, executorResult.SrcFileCount);
            Assert.Equal(27, Directory.GetFiles(_target, "*", SearchOption.AllDirectories).Length);
            Assert.Equal(2, Directory.GetDirectories(_target, "*", SearchOption.AllDirectories).Length);
        }

        [Fact]
        public async Task Test5_Cleanup_On_With_Same_File()
        {
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_target, 1, 10, new byte[1000]);
            var srcFolder1 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder1, 1, 10, new byte[2000]);
            var srcFolder2 = TestHelper.GetFolderPath(_src);
            await TestHelper.CreateRandomFileAsync(srcFolder2, 1, 10, new byte[3000]);
            var targetFolder1 = TestHelper.GetFolderPath(_target);
            await TestHelper.CreateRandomFileAsync(targetFolder1, 10, 19, new byte[5000000]);

            _options.Cleanup = true;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(27, executorResult.TargetAffectedFileCount);
            Assert.Equal(2, executorResult.SrcDirCount);
            Assert.Equal(27, executorResult.SrcFileCount);
            Assert.Equal(27, Directory.GetFiles(_target, "*", SearchOption.AllDirectories).Length);
            Assert.Equal(2, Directory.GetDirectories(_target, "*", SearchOption.AllDirectories).Length);
        }

        [Fact]
        public async Task Test6_Force_On()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 10, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Force = true;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(9, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(9, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test7_Force_Off()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 10, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Force = false;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(4, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(9, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test8_Strict_On()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 10, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Strict = true;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(9, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(9, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test9_Strict_On()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 6, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_target, 6, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Strict = true;
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(5, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(9, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test10_Include()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 6, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_target, 6, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Include = new List<string> {"2*", "3*"};
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(2, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(7, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test11_Exclude()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 6, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_target, 6, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 10, new byte[1000]);

            _options.Exclude = new List<string> {"2*"};
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(3, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(9, executorResult.SrcFileCount);
            Assert.Equal(8, Directory.GetFiles(_target).Length);
        }

        [Fact]
        public async Task Test12_Include_Exclude()
        {
            await TestHelper.CreateRandomFileAsync(_target, 5, 6, new byte[1000 * 1000]);
            await TestHelper.CreateRandomFileAsync(_target, 6, 10, new byte[1000]);
            await TestHelper.CreateRandomFileAsync(_src, 1, 18, new byte[1000]);

            _options.Exclude = new List<string> {"12*"};
            _options.Include = new List<string> {"2*"};
            var executorResult = await _executor.ExecuteAsync();
            Assert.True(executorResult.Succeed);
            Assert.Equal(1, executorResult.TargetAffectedFileCount);
            Assert.Equal(0, executorResult.SrcDirCount);
            Assert.Equal(17, executorResult.SrcFileCount);
            Assert.Equal(6, Directory.GetFiles(_target).Length);
        }
    }
}