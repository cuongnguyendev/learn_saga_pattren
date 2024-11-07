
EXEC sp_addlinkedserver 
    @server = 'RemoteStockService',  -- Use a friendly name for the linked server
    @srvproduct = '', 
    @provider = 'SQLNCLI',  -- SQL Native Client
    @datasrc = '192.168.21.15,1445';  -- IP address and custom port

	SELECT * 
FROM OPENQUERY(RemoteStockService, 'SELECT * FROM StockService.dbo.Stocks');
