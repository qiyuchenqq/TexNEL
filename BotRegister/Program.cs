using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using Serilog;
using Tex.Core.Api;

namespace BotRegister;

class Program
{
    static async Task Main(string[] args)
    {
        // ������־
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("С��ע���������...");

        try
        {
            // OxygenApiͨ����̬����ģʽ�Զ���ʼ��

            // �����˺Ŵ洢Ŀ¼
            var dataDir = Path.Combine(AppContext.BaseDirectory, "accounts");
            Directory.CreateDirectory(dataDir);

            Console.WriteLine("==================================");
            Console.WriteLine("          С��ע���");
            Console.WriteLine("==================================");
            Console.WriteLine("1. ע�ᵥ��С��");
            Console.WriteLine("2. ����ע��С��");
            Console.WriteLine("3. �鿴��ע���˺�");
            Console.WriteLine("4. �˳�");
            Console.WriteLine("==================================");
            Console.Write("��ѡ�����: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await RegisterSingleAccount(dataDir);
                    break;
                case "2":
                    await RegisterBatchAccounts(dataDir);
                    break;
                case "3":
                    ViewAccounts(dataDir);
                    break;
                case "4":
                    Log.Information("�˳�����...");
                    return;
                default:
                    Log.Warning("��Ч��ѡ�����������г���");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "�������г���");
        }
        finally
        {
            Log.CloseAndFlush();
            Console.WriteLine("��������˳�...");
            Console.ReadKey();
        }
    }

    static async Task RegisterSingleAccount(string dataDir)
    {
        Log.Information("��ʼע�ᵥ��С��...");

        // ��������˺���Ϣ
        var accountInfo = GenerateRandomAccount();
        Log.Information($"�����˺�: {accountInfo.Username}");
        Log.Information($"��������: {accountInfo.Email}");
        Log.Information($"��������: {accountInfo.Password}");

        // ע���˺�
        var result = await RegisterAccount(accountInfo);
        if (result.Success)
        {
            Log.Information("ע��ɹ���");
            // �����˺���Ϣ
            SaveAccount(dataDir, accountInfo);
        }
        else
        {
            Log.Error("ע��ʧ��: {Message}", result.Message);
        }
    }

    static async Task RegisterBatchAccounts(string dataDir)
    {
        Console.Write("������Ҫע����˺�����: ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Log.Warning("��Ч����������������������");
            return;
        }

        Log.Information($"��ʼ����ע�� {count} ��С��...");

        int successCount = 0;
        for (int i = 0; i < count; i++)
        {
            Log.Information($"ע��� {i + 1} ���˺�...");

            // ��������˺���Ϣ
            var accountInfo = GenerateRandomAccount();
            Log.Information($"�����˺�: {accountInfo.Username}");

            // ע���˺�
            var result = await RegisterAccount(accountInfo);
            if (result.Success)
            {
                Log.Information("ע��ɹ���");
                // �����˺���Ϣ
                SaveAccount(dataDir, accountInfo);
                successCount++;
            }
            else
            {
                Log.Error("ע��ʧ��: {Message}", result.Message);
            }

            // ��ֹ�������Ƶ��
            await Task.Delay(1000);
        }

        Log.Information($"����ע����ɣ��ɹ� {successCount} ����ʧ�� {count - successCount} ��");
    }

    static void ViewAccounts(string dataDir)
    {
        var files = Directory.GetFiles(dataDir, "*.json");
        if (files.Length == 0)
        {
            Log.Information("������ע����˺�");
            return;
        }

        Log.Information($"��ע���˺� ({files.Length} ��):");
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                var account = JsonSerializer.Deserialize<AccountInfo>(content);
                if (account != null)
                {
                    Log.Information($"�˺�: {account.Username}, ����: {account.Email}, ����: {account.Password}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "��ȡ�˺��ļ�ʧ��: {File}", file);
            }
        }
    }

    static AccountInfo GenerateRandomAccount()
    {
        var random = new Random();
        var username = "bot_" + Guid.NewGuid().ToString().Substring(0, 8);
        var email = $"{username}@example.com";
        var password = GenerateRandomPassword(8);

        return new AccountInfo
        {
            Username = username,
            Email = email,
            Password = password
        };
    }

    static string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var password = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }
        return password.ToString();
    }

    static async Task<RegisterResult> RegisterAccount(AccountInfo accountInfo)
    {
        try
        {
            // ����ע���ʼ�
            var mailResult = await OxygenApi.Instance.SendRegisterMailAsync(accountInfo.Email);
            if (!mailResult.Success)
            {
                return new RegisterResult(false, mailResult.Message ?? "����ע���ʼ�ʧ��");
            }

            // ����򻯴�����ʵ����Ҫ�û�������֤��
            // �������Զ�ע��������Ǽ�����֤����֤ͨ��
            var verifyResult = await OxygenApi.Instance.VerifyCodeAsync(accountInfo.Email, "123456");
            if (!verifyResult.Success)
            {
                return new RegisterResult(false, verifyResult.Message ?? "��֤����֤ʧ��");
            }

            // ���ע��
            var registerResult = await OxygenApi.Instance.RegisterAsync(accountInfo.Email, accountInfo.Username, accountInfo.Password);
            if (!registerResult.Success)
            {
                return new RegisterResult(false, registerResult.Message ?? "ע��ʧ��");
            }

            return new RegisterResult(true, "ע��ɹ�", registerResult.Token);
        }
        catch (Exception ex)
        {
            return new RegisterResult(false, "ע����̳���: " + ex.Message);
        }
    }

    static void SaveAccount(string dataDir, AccountInfo accountInfo)
    {
        try
        {
            var fileName = Path.Combine(dataDir, $"{accountInfo.Username}.json");
            var content = JsonSerializer.Serialize(accountInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, content, Encoding.UTF8);
            Log.Information("�˺���Ϣ�ѱ��浽: {File}", fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "�����˺���Ϣʧ��");
        }
    }
}

class AccountInfo
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Token { get; set; }
}

class RegisterResult
{
    public bool Success { get; }
    public string Message { get; }
    public string? Token { get; }

    public RegisterResult(bool success, string message, string? token = null)
    {
        Success = success;
        Message = message;
        Token = token;
    }
}
