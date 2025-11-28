using System; // Tipos base e I/O (Console, exceções, etc.).
using System.Collections.Generic; // Listas genéricas (List<T>, IReadOnlyList<T>) usadas para armazenar ASCII, chaves.
using System.Linq;  // Extensões LINQ (Select, ToList) para transformar sequências rapidamente.
using System.Numerics; // BigInteger para cálculos com números grandes do RSA.
using System.Security.Cryptography; // RandomNumberGenerator criptograficamente seguro para gerar primos/coeficientes.
using System.Text; // Encoding.ASCII para converter mensagem ↔ valores numéricos.

namespace RsaConsoleApp;

// Programa de console que demonstra passo a passo o algoritmo RSA
internal static class Program
{
    // Define o tamanho de cada primo usado na geracao da chave (n ~ 512 bits)
    private const int PrimeBitLength = 256;

    private static void Main()
    {
        // Mantem a execucao em loop para permitir varios testes seguidos
        while (true)
        {
            RunOnce();
            if (!ShouldContinue())
            {
                Console.WriteLine("Encerrando o programa. Ate logo!");
                break;
            }

            Console.WriteLine();
        }
    }

    // Executa uma rodada completa: leitura da mensagem e fluxo RSA
    private static void RunOnce()
    {
        // Pergunta ao usuario qual mensagem sera criptografada
        Console.Write("Digite a mensagem (ENTER para usar \"Ola Mundo!\"): ");
        var userInput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(userInput))
        {
            userInput = "Ola Mundo!";
            Console.WriteLine($"Mensagem padrao utilizada: {userInput}");
        }

        // Converte a entrada em ASCII e aplica geracao de chaves, cripto e decripto
        var asciiValues = ConvertToAscii(userInput);
        var keyPair = RsaService.GenerateKeyPair(PrimeBitLength);
        var encryptedValues = RsaService.Encrypt(asciiValues, keyPair.PublicKey);
        var decryptedAscii = RsaService.Decrypt(encryptedValues, keyPair.PrivateKey);
        var decryptedMessage = ConvertToText(decryptedAscii);

        // Funcoes locais para padronizar o visual da saida
void Title(string text)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(text);
    Console.ResetColor();
}

// Imprime um rotulo em ciano antes do valor correspondente
void Label(string text)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(text);
    Console.ResetColor();
}

Console.WriteLine();
Title("=========== RESULTADOS RSA ===========\n");

Label("Mensagem original         : "); 
Console.WriteLine(userInput);

Label("Valores ASCII             : ");
Console.WriteLine(FormatValues(asciiValues));

Console.WriteLine();
Title("----- CHAVES -----");
Label("Chave pública  (n, e)     : ");
Console.WriteLine($"({keyPair.PublicKey.Modulus}, {keyPair.PublicKey.Exponent})");

Label("Chave privada  (n, d)     : ");
Console.WriteLine($"({keyPair.PrivateKey.Modulus}, {keyPair.PrivateKey.Exponent})");

Console.WriteLine();
Title("----- CRIPTOGRAFIA -----");
Label("Mensagem criptografada    : ");
Console.WriteLine(FormatValues(encryptedValues));

Console.WriteLine();
Title("----- DESCRIPTOGRAFIA -----");
Label("ASCII descriptografado    : ");
Console.WriteLine(FormatValues(decryptedAscii));

Label("Mensagem final (texto)    : ");
Console.WriteLine(decryptedMessage);

Console.WriteLine();
Title("======================================");

    }

    // Questiona o usuario se deseja continuar no loop principal
    private static bool ShouldContinue()
    {
        while (true)
        {
            Console.Write("Deseja continuar? Digite 0 para continuar ou 1 para sair: ");
            var input = Console.ReadLine();

            if (input == "0")
            {
                return true;
            }

            if (input == "1")
            {
                return false;
            }

            Console.WriteLine("Opcao invalida. Tente novamente.");
        }
    }

    // Converte o texto digitado para a lista de valores ASCII
    private static IReadOnlyList<int> ConvertToAscii(string message)
    {
        var bytes = Encoding.ASCII.GetBytes(message);
        return bytes.Select(b => (int)b).ToList();
    }

    // Reconverte uma lista de inteiros ASCII para texto normal
    private static string ConvertToText(IEnumerable<int> asciiValues)
    {
        var bytes = asciiValues.Select(value => (byte)value).ToArray();
        return Encoding.ASCII.GetString(bytes);
    }

    // Utilitario para exibir sequencias na tela
    private static string FormatValues<T>(IEnumerable<T> values) => string.Join(" ", values);
}

// Servico com todas as rotinas relacionadas ao algoritmo RSA
internal static class RsaService
{
    // Tabela de primos pequenos usada para eliminar candidatos obvios
    private static readonly int[] SmallPrimes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59 };
    // Quantidade de iteracoes do teste de Miller-Rabin para cada candidato
    private const int MillerRabinRounds = 12;
    // Expoente publico padrao mais comum utilizado em RSA
    private const int DefaultPublicExponent = 65537;

    // Gera p, q, calcula n e monta o par de chaves publica/privada
    public static RsaKeyPair GenerateKeyPair(int primeBitLength)
    {
        using var rng = RandomNumberGenerator.Create();
        var p = GeneratePrime(primeBitLength, rng);
        BigInteger q;
        do
        {
            q = GeneratePrime(primeBitLength, rng);
        } while (q == p);

        var n = p * q;
        var phi = (p - 1) * (q - 1);

        var e = new BigInteger(DefaultPublicExponent);
        if (BigInteger.GreatestCommonDivisor(e, phi) != BigInteger.One)
        {
            e = FindCoprime(phi, rng);
        }

        var d = ModInverse(e, phi);
        return new RsaKeyPair(new RsaKey(n, e), new RsaKey(n, d));
    }

    // Aplica c = m^e mod n para cada valor ASCII
    public static List<BigInteger> Encrypt(IEnumerable<int> asciiValues, RsaKey publicKey)
    {
        return asciiValues
            .Select(value => BigInteger.ModPow(value, publicKey.Exponent, publicKey.Modulus))
            .ToList();
    }

    // Aplica m = c^d mod n e retorna novamente em valores ASCII
    public static List<int> Decrypt(IEnumerable<BigInteger> cipherValues, RsaKey privateKey)
    {
        return cipherValues
            .Select(value => (int)BigInteger.ModPow(value, privateKey.Exponent, privateKey.Modulus))
            .ToList();
    }

    // Gera candidatos impares ate encontrar um primo provavel
    private static BigInteger GeneratePrime(int bitLength, RandomNumberGenerator rng)
    {
        while (true)
        {
            var candidate = RandomOddBigInteger(bitLength, rng);
            if (IsProbablyPrime(candidate, rng))
            {
                return candidate;
            }
        }
    }

    // Implementa Miller-Rabin com algumas otimizacoes iniciais
    private static bool IsProbablyPrime(BigInteger value, RandomNumberGenerator rng)
    {
        if (value < 2)
        {
            return false;
        }

        if (value == 2 || value == 3)
        {
            return true;
        }

        if (value.IsEven)
        {
            return false;
        }

        foreach (var prime in SmallPrimes)
        {
            if (value == prime)
            {
                return true;
            }

            if (value % prime == 0)
            {
                return false;
            }
        }

        var d = value - 1;
        var s = 0;
        while (d.IsEven)
        {
            d /= 2;
            s++;
        }

        for (var i = 0; i < MillerRabinRounds; i++)
        {
            var a = RandomInRange(2, value - 2, rng);
            var x = BigInteger.ModPow(a, d, value);
            if (x == 1 || x == value - 1)
            {
                continue;
            }

            var continueWitnessLoop = false;
            for (var r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, value);
                if (x == value - 1)
                {
                    continueWitnessLoop = true;
                    break;
                }
            }

            if (continueWitnessLoop)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    // Cria um BigInteger impar com o numero de bits solicitado
    private static BigInteger RandomOddBigInteger(int bitLength, RandomNumberGenerator rng)
    {
        if (bitLength < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "O tamanho em bits deve ser pelo menos 16.");
        }

        var byteCount = (bitLength + 7) / 8;
        var bytes = new byte[byteCount];
        rng.GetBytes(bytes);

        var unusedBits = byteCount * 8 - bitLength;
        if (unusedBits > 0)
        {
            var mask = (byte)(0xFF >> unusedBits);
            bytes[^1] &= mask;
        }

        var highestBitIndex = bitLength - 1;
        var byteIndex = highestBitIndex / 8;
        var bitIndex = highestBitIndex % 8;
        bytes[byteIndex] |= (byte)(1 << bitIndex);
        bytes[0] |= 1;

        var extended = new byte[bytes.Length + 1];
        Array.Copy(bytes, extended, bytes.Length);
        return new BigInteger(extended);
    }

    // Gera uniformemente numeros dentro do intervalo especificado
    private static BigInteger RandomInRange(BigInteger minInclusive, BigInteger maxInclusive, RandomNumberGenerator rng)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentException("Intervalo invalido.");
        }

        var range = maxInclusive - minInclusive + BigInteger.One;
        var rangeBytes = range.ToByteArray();
        BigInteger result;

        do
        {
            rng.GetBytes(rangeBytes);
            rangeBytes[^1] &= 0x7F;
            result = new BigInteger(rangeBytes);
        } while (result >= range || result < 0);

        return minInclusive + result;
    }

    // Busca um valor que seja coprimo a phi para atuar como expoente
    private static BigInteger FindCoprime(BigInteger phi, RandomNumberGenerator rng)
    {
        BigInteger candidate;
        do
        {
            candidate = RandomInRange(3, phi - 1, rng);
        } while (BigInteger.GreatestCommonDivisor(candidate, phi) != BigInteger.One);

        return candidate;
    }

    // Calcula o inverso modular usando o algoritmo estendido de Euclides
    private static BigInteger ModInverse(BigInteger value, BigInteger modulus)
    {
        var t = BigInteger.Zero;
        var newT = BigInteger.One;
        var r = modulus;
        var newR = value % modulus;

        while (newR != 0)
        {
            var quotient = r / newR;
            (t, newT) = (newT, t - quotient * newT);
            (r, newR) = (newR, r - quotient * newR);
        }

        if (r > BigInteger.One)
        {
            throw new InvalidOperationException("Valor nao inversivel modulo phi.");
        }

        if (t < 0)
        {
            t += modulus;
        }

        return t;
    }
}

// Representa uma chave individual (publica ou privada)
internal sealed class RsaKey
{
    public RsaKey(BigInteger modulus, BigInteger exponent)
    {
        Modulus = modulus;
        Exponent = exponent;
    }

    public BigInteger Modulus { get; }

    public BigInteger Exponent { get; }
}

// Contem o par de chaves gerado a partir de p e q
internal sealed class RsaKeyPair
{
    public RsaKeyPair(RsaKey publicKey, RsaKey privateKey)
    {
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public RsaKey PublicKey { get; }

    public RsaKey PrivateKey { get; }
}
