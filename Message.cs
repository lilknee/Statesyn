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
    /// ʣ�೤��
    /// </summary>
    public int RemSize { get { return buffer.Length - startIndex; } }

    /// <summary>
    /// ����Buffer�е���Ϣ �������������Ϣ����ص���������
    /// </summary>
    /// <param name="len"></param>
    /// <param name="HandleResponse"></param>
    public void ReadBuffer(int len, Action<string> HandleResponse)
    {
        // ��������Ϊ����buffer����
        startIndex += len;
        while (true)
        {
            // ȷ��buffer��������һ����Ϣͷ
            if (startIndex <= 4)
                return;

            // ��ȡ��Ϣ��ĳ��ȣ�������Ϣ��ĳ��ȴ洢��ǰ4���ֽ��У�
            int count = BitConverter.ToInt32(buffer, 0);

            // ȷ��buffer�а���������һ����Ϣ
            if (startIndex >= count + 4)
            {
                // ��buffer����ȡ��Ϣ�壨������4��ʼ������Ϊcount��
                string message = Encoding.UTF8.GetString(buffer, 4, count);
                HandleResponse(message);

                // ��ʣ��������ƶ���buffer�Ŀ�ʼλ��
                Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);

                // ����startIndex����ȥ�Ѿ����������Ϣ����
                startIndex -= count + 4;
            }
            else
            {
                // ���û��������һ����Ϣ��������ѭ��
                break;
            }
        }
    }

    /// <summary>
    /// ���ַ�����Ϣת��ΪTCPЭ�鷢��������ֽ�����
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] PackToData(string message)
    {
        // ����Ϣת��Ϊ�ֽ����飨UTF8���룩
        byte[] body = Encoding.UTF8.GetBytes(message);

        // ��ȡ��Ϣ�峤�Ȳ����������Ϣͷ��4���ֽڣ�
        byte[] head = BitConverter.GetBytes(body.Length);

        // ����Ϣͷ����Ϣ��ƴ����һ�𲢷���
        return head.Concat(body).ToArray();
    }

    /// <summary>
    /// ���ַ�����Ϣת��ΪUDPЭ�鷢��������ֽ�����
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] PackToDataUDP(string message)
    {
        // UDPЭ������������Ϣͷ����Ϣ�壬ֱ��ת��Ϊ�ֽ����鲢����
        return Encoding.UTF8.GetBytes(message);
    }
}
