using System;
using System.Linq;
using System.Text;

public class Message
{
    private byte[] buffer = new byte[1024];
    private int startIndex;
    public byte[] Buffer { get { return buffer; } }
    public int StartIndex { get { return startIndex; } }
    /// <summary>
    /// 剩余长度
    /// </summary>
    public int RemSize { get { return buffer.Length - startIndex; } }

    /// <summary>
    /// 解析Buffer中的消息 并将解析后的消息传入回调函数调用
    /// </summary>
    /// <param name="len"></param>
    /// <param name="HandleResponse"></param>
    public void ReadBuffer(int len, Action<string> HandleResponse)
    {
        // 更新索引为整个buffer长度
        startIndex += len;
        while (true)
        {
            // 确保buffer中至少有一个消息头
            if (startIndex <= 4)
                return;

            // 读取消息体的长度（假设消息体的长度存储在前4个字节中）
            int count = BitConverter.ToInt32(buffer, 0);

            // 确保buffer中包含完整的一条消息
            if (startIndex >= count + 4)
            {
                // 从buffer中提取消息体（从索引4开始，长度为count）
                string message = Encoding.UTF8.GetString(buffer, 4, count);
                HandleResponse(message);

                // 将剩余的数据移动到buffer的开始位置
                Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);

                // 更新startIndex，减去已经处理过的消息长度
                startIndex -= count + 4;
            }
            else
            {
                // 如果没有完整的一条消息，则跳出循环
                break;
            }
        }
    }

    /// <summary>
    /// 将字符串消息转换为TCP协议发送所需的字节数据
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] PackToData(string message)
    {
        // 将消息转换为字节数组（UTF8编码）
        byte[] body = Encoding.UTF8.GetBytes(message);

        // 获取消息体长度并将其存入消息头（4个字节）
        byte[] head = BitConverter.GetBytes(body.Length);

        // 将消息头和消息体拼接在一起并返回
        return head.Concat(body).ToArray();
    }

    /// <summary>
    /// 将字符串消息转换为UDP协议发送所需的字节数据
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] PackToDataUDP(string message)
    {
        // UDP协议无需区分消息头和消息体，直接转换为字节数组并返回
        return Encoding.UTF8.GetBytes(message);
    }
}
