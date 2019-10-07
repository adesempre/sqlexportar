# sqlexportar
Projeto C3 para exportação de objetos SQL Server

Serviço de exportação de arquivos de script de criação de tabelas, views e procedures de bancos de dados SQL Server, adiciona e versiona em um repositório GIT.

É preciso configurar a arquivo de configuração da aplicação:
-connectionString: string padrão de conexão que será usada. A configuração versiona está configurada para usar a autenticação integrada do Windows. É possível usar as palavras chaves [servidor] e [banco], que serão substituidas em tempo de execução pela instância SQL e banco de dados que estiverem parametrizados nas propriedades abaixo;

-ServidorSQL: lista, separada por ";", das instâncias que será conectadas;

-BancoSQL: lista, separada por ";", de banco de dados que será conectados, espera-se que o banco exista para todas as instâncias parametrizadas;

-LocalScript: caminho completo para geração do script. É possível usar palavras-chave para definição do nome do script:
*[servidor] = nome da instância SQL de onde o objeto está sendo exportado;
*[banco] = nome do banco de dados de onde o objeto está sendo exportado;
*[tipo] = tipo de objeto exportado (table, view, procedure);
*[proprietario] = proprietário (owner) do objeto;
*[nome] = nome do objeto exportado.

-HorarioExecucaoExportacaoSQL: horário de execução do serviço;
-NDiasModificacaoSQL: número de dias em que a modificação do objeto foi realizada. Ex: data atual 15/10/2019, se a parametrização for 2 dias, serão exportados os objetos alterados a partir de 13/10/2019.

-comandoGIT: caminho e nome completo do aplicativo git para versionamento;
repositorioGIT: caminho da pasta de trabalho do projeto versionado.

A aplicação utiliza NLog e está com exemplo configurado para gerar os arquivos em "logs/SqlExporter.log".

Para instalação do serviço no Windows:
-abra um prompt de comando em modo administrador.
-execute o comando: sc create "SqlExporter" binPath="<caminho-pasta-instalação>\SqlExporter.exe" start=delayed-auto error=ignore DisplayName="Serviço de exportação de objetos do SQL Server"

Se a forma de conexão com o banco de dados estiver integrada com o Windows, será prefico definir a conta responsável pela conexão na aba de Logon das propriedades do Serviço.
