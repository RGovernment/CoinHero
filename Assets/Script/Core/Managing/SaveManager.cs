using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Constants;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SaveDir;
    public bool IsSaving { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 입력된 파일 주소를 재사용하기 위해 초기화
        SaveDir = Path.Combine(Application.persistentDataPath, SAVE_FILE_ROOT_NAME);

        // 세이브 폴더가 없으면 생성
        Directory.CreateDirectory(SaveDir);
    }

    /// <summary>
    /// 저장
    /// </summary>
    public async UniTask Save()
    {
        IsSaving = true;

        var state = GameManager.Instance.state;

        var data = new SaveData
        {
            state = JsonConvert.SerializeObject(state)
        };

        // 데이터 저장
        await UniTask.RunOnThreadPool(() =>
        {
            WriteEncrypted(GetSavePath(), data);
        });

        IsSaving = false;
    }

    /// <summary>
    /// 불러오기
    /// </summary>
    public void Load()
    {
        string path = GetSavePath();
        if (!File.Exists(path)) return;

        SaveData data = ReadEncrypted<SaveData>(path);
        DataReload(data);
    }

    /// <summary>
    /// 로드한 데이터를 GameManager에 적용
    /// </summary>
    /// <param name="data"></param>
    private void DataReload(SaveData data)
    {
        string stateString = data.state;
        var state = JsonConvert.DeserializeObject<GameState>(stateString);
        GameManager.Instance.UpdateState(state);
    }

    /// <summary>
    /// 세이브 파일이 존재하는지 확인
    /// </summary>
    public bool SaveExists() => File.Exists(GetSavePath());

    /// <summary>
    /// 세이브 파일 삭제
    /// </summary>
    public void DeleteSave()
    {
        var path = GetSavePath();

        if (File.Exists(path))
            File.Delete(path);
        
    }

    /// <summary>
    /// 주소 재사용을 위해 path를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private string GetSavePath()
    {
        if (string.IsNullOrEmpty(SaveDir))
            SaveDir = Path.Combine(Application.persistentDataPath, SAVE_FILE_ROOT_NAME);

        return Path.Combine(SaveDir, SAVE_FILE_NAME);
    }


    /// <summary>
    /// 암호화된 데이터를 파일에 쓰는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="data"></param>
    private void WriteEncrypted<T>(string path, T data)
    {
        string json = JsonConvert.SerializeObject(data);
        string encrypted = Encrypt(json);
        File.WriteAllText(path, encrypted);
    }

    /// <summary>
    /// 암호화된 데이터를 파일에서 읽어오는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private T ReadEncrypted<T>(string path)
    {
        string encrypted = File.ReadAllText(path);
        string json = Decrypt(encrypted);
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 암호화
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>

    private string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(GetKey());
        aes.IV = Encoding.UTF8.GetBytes(GetIV());
        using var encryptor = aes.CreateEncryptor();
        byte[] input = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// 복호화
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns>복호화된 문자열</returns>
    private string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(GetKey());
        aes.IV = Encoding.UTF8.GetBytes(GetIV());
        using var decryptor = aes.CreateDecryptor();
        byte[] input = Convert.FromBase64String(cipherText);
        byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// 암호화용 키 반환
    /// </summary>
    /// <returns>키 값</returns>
    private string GetKey()
    {
        byte[] bytes = new byte[]
        {
            0x54, 0x68, 0x61, 0x6E,
            0x6B, 0x73, 0x50, 0x6C,
            0x61, 0x79, 0x4D, 0x79,
            0x47, 0x61, 0x6D, 0x65
        };
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 암호화용 IV 반환
    /// </summary>
    /// <returns>IV값</returns>
    private string GetIV()
    {
        byte[] bytes = new byte[]
        {
            0x41, 0x6E, 0x64, 0x50,
            0x6C, 0x65, 0x61, 0x73,
            0x65, 0x48, 0x61, 0x76,
            0x65, 0x46, 0x75, 0x6E
        };
        return Encoding.UTF8.GetString(bytes);
    }
}