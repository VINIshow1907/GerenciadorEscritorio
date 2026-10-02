# 💼 Sistema de Controle Financeiro (ERP)

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Windows Forms](https://img.shields.io/badge/Windows%20Forms-0078D7?style=for-the-badge&logo=windows&logoColor=white)

## 📌 Sobre o Projeto
Software desktop desenvolvido sob medida para resolver os desafios reais de gestão financeira e fluxo de caixa de um escritório de serviços administrativos e fiscais. O sistema otimiza o registro de honorários, controle de despesas operacionais e, principalmente, o rastreamento de taxas e DARFs pagos em nome dos clientes para posterior reembolso.

O projeto demonstra a aplicação de conceitos de Engenharia de Software no desenvolvimento de uma arquitetura robusta, segura e focada na experiência do usuário final.

## 🚀 Principais Funcionalidades

* **Dashboard Analítico:** Visão em tempo real do caixa do escritório (Receitas, Despesas e Saldo Líquido) com interface limpa e intuitiva.
* **Geração Avançada de Relatórios (PDF):** Integração com a biblioteca iText7 para emissão de extratos profissionais.
  * *Visão do Caixa:* Balanço de entradas e saídas com gráficos de distribuição de despesas.
  * *Extrato de Cobrança:* Cálculo automático de dívidas ativas de clientes, abatendo reembolsos já efetuados.
  * *Histórico do Cliente:* Transparência total de gastos (honorários e impostos gerados).
* **Filtros Dinâmicos de Dados:** Consultas SQL otimizadas com parâmetros dinâmicos (`BETWEEN` para períodos de datas personalizados, cláusulas `IN` para múltiplas categorias).
* **Controle de Sessão e Operadores:** Rastreabilidade de ações por usuário logado.

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET)
* **Interface:** Windows Forms (WinForms)
* **Banco de Dados:** PostgreSQL (Acesso via Npgsql)
* **Geração de PDF:** iText7
* **Segurança:** Algoritmo de criptografia customizado para ofuscação de credenciais locais (JSON).

## 🔒 Arquitetura e Segurança
O sistema adota boas práticas de segurança para ambientes corporativos. O arquivo de configuração de rede e credenciais do banco de dados (`config_db.json`) é criptografado localmente, impedindo a exposição de senhas em texto plano. *(Nota: Por questões de segurança, o arquivo de credenciais e os dados reais não integram este repositório público).*

## 📸 Demonstração do Sistema

TELA DE DASBOARD
<img width="1917" height="1078" alt="image" src="https://github.com/user-attachments/assets/e662f2f1-8c67-4a56-a278-abeff2658a34" />

------
TELA DE LANÇAMENTOS
<img width="1917" height="990" alt="image" src="https://github.com/user-attachments/assets/3462686b-adbb-4f3c-8530-b00831db1961" />

-------
TELA DE SAÍDA
<img width="1917" height="976" alt="image" src="https://github.com/user-attachments/assets/bcbfe67d-4d66-43c1-a262-43774d2efb55" />

-------
TELA HISTÓRICO 
<img width="1917" height="1078" alt="image" src="https://github.com/user-attachments/assets/e50704b6-e295-4a6c-b258-d80f2b22291d" />

---------
TELA GERAÇÃO DE RELATÓRIOS
<img width="1917" height="1078" alt="image" src="https://github.com/user-attachments/assets/90498c96-54c1-4800-ba90-6ee5b53a886a" />

------
PDF DO RELATÓRIO GERADO

<img width="552" height="781" alt="image" src="https://github.com/user-attachments/assets/09c17892-9494-44bc-8285-2b192575a4f5" />


---

## 👨‍💻 Autor

**Vinicius Lúcio Marcolino da Silva**  
Desenvolvedor de Software | Análise e Desenvolvimento de Sistemas

🔗 [LinkedIn](www.linkedin.com/in/vinicius-lucio)
🔗 [E-mail](viniciuslucio792@gmail.com)
