using LogComponent;
using Moq;
using System;
using System.Threading;
using Xunit;
namespace LogComponentTest
{
    public class LoggerTest
    {
        [Fact]
        public void Write_OneLine()
        {
            var buffer = "";
            var moq = new Mock<ILogWriter>();
            moq.Setup(w => w.Write(It.IsAny<string>())).Callback<string>((text) => buffer += text);
            var logger = new Logger(moq.Object);
            const string text = "awesome test";

            logger.Write(text);
            Thread.Sleep(100);

            moq.Verify(w => w.Write(It.IsAny<string>()), Times.Once);
            Assert.Contains(text, buffer);
            Assert.Equal(2, buffer.Split(Environment.NewLine).Length); //last line should be empty
            Assert.Equal("", buffer.Split(Environment.NewLine)[1]);
        }

        [Fact]
        public void Write_TwoLine()
        {
            var buffer = "";
            var moq = new Mock<ILogWriter>();
            moq.Setup(w => w.Write(It.IsAny<string>())).Callback<string>((text) => buffer += text);
            var logger = new Logger(moq.Object);
            const string text = "awesome test";
            const string text1 = "awesome TEST";

            logger.Write(text);
            logger.Write(text1);
            Thread.Sleep(100);

            moq.Verify(w => w.Write(It.IsAny<string>()), Times.Exactly(2));
            Assert.Contains(text, buffer);
            Assert.Contains(text1, buffer);
            Assert.Equal(3, buffer.Split(Environment.NewLine).Length); //last line should be empty
        }

        [Fact]
        public void Write_ThrowsError()
        {
            var buffer = "";
            var moq = new Mock<ILogWriter>();
            const string text1 = "awesome test1";
            const string text2 = "awesome test2";
            const string errText = "error";
            moq.Setup(w => w.Write(It.IsNotIn(errText))).Callback<string>((text) => buffer += text);
            moq.Setup(w => w.Write(It.Is<string>(s => s.Contains(errText)))).Throws(new Exception("problem"));
            var logger = new Logger(moq.Object);

            logger.Write(text1);
            logger.Write(errText);
            logger.Write(text2);

            Thread.Sleep(100);

            Assert.Contains(text1, buffer);
            Assert.Contains(text2, buffer);
            Assert.DoesNotContain(errText, buffer);
        }

        [Fact]
        public void StopWithFlush()
        {
            var buffer = "";
            var moq = new Mock<ILogWriter>();
            moq.Setup(w => w.Write(It.IsAny<string>())).Callback<string>((text) => buffer += text);
            var logger = new Logger(moq.Object);
            const string text = "Line number";

            const int maxLine = 20000;
            for (int i = 0; i < maxLine; i++)
            {
                logger.Write($"{text}:{i}");
            }
            logger.StopWithFlush();

            moq.Verify(w => w.Write(It.IsAny<string>()), Times.Exactly(maxLine));
            moq.Verify(w => w.Flush(), Times.Once);
            Assert.Contains($"{text}:{maxLine - 1}", buffer);//check if the last log was appended
        }

        [Fact]
        public void StopWithoutFlush()
        {
            var buffer = "";
            var moq = new Mock<ILogWriter>();
            moq.Setup(w => w.Write(It.IsAny<string>())).Callback<string>((text) => buffer += text);
            var logger = new Logger(moq.Object);
            const string text = "Line number";

            const int maxLine = 20000;
            for (int i = 0; i < maxLine; i++)
            {
                logger.Write($"{text}:{i}");
            }
            logger.StopWithoutFlush();

            moq.Verify(w => w.Write(It.IsAny<string>()), Times.AtMost(maxLine));
            moq.Verify(w => w.Flush(), Times.Never);
            Assert.DoesNotContain($"{text}:{maxLine - 1}", buffer);//check if the last log is not appended
        }
    }
}
