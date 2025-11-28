# Componentes Utilizados no Projeto

Este documento apresenta, de forma simples e didática, os principais elementos envolvidos na implementação do algoritmo RSA no console em C#.

---

## **1. Ambiente e Linguagem Utilizados**

- **C# (.NET 8)**  
  Linguagem usada para construir o programa. Ela possui suporte a operações com números grandes, essenciais para o RSA.

- **.NET SDK**  
  Conjunto de ferramentas que permite compilar e executar o projeto, incluindo:
  - `dotnet CLI`
  - Compilador C#
  - Bibliotecas padrão do .NET

---

## **2. Bibliotecas do .NET Utilizadas**

Estas bibliotecas fazem parte da Base Class Library (BCL), ou seja, não exigem instalação adicional.

- **`System`**  
  Recursos básicos como entrada e saída de dados.

- **`System.Collections.Generic`**  
  Manipulação de coleções (listas, sequências etc.).

- **`System.Linq`**  
  Facilita transformações e operações sobre listas e sequências.

- **`System.Numerics`**  
  Contém o tipo **BigInteger**, essencial para cálculos de grandes números usados no RSA.

- **`System.Security.Cryptography`**  
  Fornece o `RandomNumberGenerator`, usado para gerar números aleatórios seguros na criação dos primos.

- **`System.Text`**  
  Responsável pela conversão entre texto e ASCII.

---

## **3. Estrutura Interna do Código**

O projeto possui alguns componentes principais:

### **• Program (arquivo principal)**  
Controla o fluxo do programa:
- Lê a mensagem digitada pelo usuário  
- Converte para ASCII  
- Gera as chaves  
- Criptografa e descriptografa  
- Exibe os resultados formatados

### **• RsaService**  
Contém **toda a lógica matemática do RSA**, incluindo:
- Geração de p e q  
- Cálculo de `n` e `phi(n)`  
- Teste de primalidade (Miller-Rabin)  
- Funções de criptografia (`Encrypt`) e descriptografia (`Decrypt`)  
- Cálculo do inverso modular (`d`)

### **• RsaKey e RsaKeyPair**  
Modelos simples que representam as chaves pública e privada.

---

## **4. Algoritmos Implementados**

### **• RSA clássico**
Implementação padrão:
- Expoente público fixo: `65537`  
- Criptografia: `c = m^e mod n`  
- Descriptografia: `m = c^d mod n`

### **• Teste de Primalidade (Miller-Rabin)**
Usado para garantir que p e q sejam **primos prováveis**.  
Foram utilizadas 12 rodadas, proporcionando alta confiabilidade.

### **• Conversão ASCII ⇄ Texto**
A mensagem é convertida para números ASCII antes da criptografia e reconstruída ao final.

---

## **5. Ferramentas Utilizadas**

- **dotnet CLI**  
  Usada para compilar e executar o projeto:
  - `dotnet build`
  - `dotnet run`

- **PowerShell**  
  Terminal usado durante o desenvolvimento e execução.

---

## **6. Observações Importantes**

- Os primos possuem **256 bits**, gerando um módulo (~512 bits) adequado para fins didáticos, mas não seguro para aplicações reais.

- Não há dependências externas: tudo foi feito usando bibliotecas padrão do .NET.

- O `RandomNumberGenerator` utilizado é criptograficamente seguro, garantindo que os números gerados não sejam previsíveis.

---

