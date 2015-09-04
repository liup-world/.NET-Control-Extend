
CREATE PROCEDURE PagingProcedure
(
  @tables      VARCHAR(300),        -- ������֧�ֶ�����ϲ�ѯ�����ŷָ���ָ������Ҫ��ȷ
  @fields      VARCHAR(500) = '*',  -- ��ѯ�ֶ��������ŷָ�
  @orderField  VARCHAR(30)  = 'ID', -- �����ֶ�
  @pageSize    INT          = 15,   -- ÿҳ��¼��������0
  @pageIndex   INT          = 0,    -- ��ǰ�ڼ�ҳ��0Ϊ��ҳ
  @where       VARCHAR(500) = '',   -- ����������WHERE
  @orderType   VARCHAR(4)   = 'ASC', -- �������ͣ�ASC/DESC
  @pageCount   INT          = 0 OUTPUT, -- ��ҳ��
  @recordCount INT          = 0 OUTPUT -- �ܼ�¼��  
)
AS
BEGIN
  SET NOCOUNT ON;

  -- �����Ϸ����
  IF (@tables IS NULL OR @tables = '')
  BEGIN
    RETURN -1;
  END

  -- assign parameters
  IF (@pageSize <= 0 OR @pageSize IS NULL)
  BEGIN
    SET @pageSize = 15;
  END

  IF (@pageIndex < 0 OR @pageIndex IS NULL)
  BEGIN
    SET @pageIndex = 0;
  END

  IF (@fields IS NULL OR @fields = '')
  BEGIN
    SET @fields = '*';
  END

  SET @where = ISNULL(@where, '');
  SET @orderField = ISNULL(@orderField, 'ID');

  DECLARE
    @nsql NVARCHAR(1000),
    @sql  VARCHAR(2000);

  -- get record count sql
  SET @nsql = '
    SELECT @recordCount=COUNT(1)
      FROM ' + @tables;
  IF (@where > '')
  BEGIN
    SET @nsql = @nsql + '
        WHERE ' + @where;
  END

  -- execute
  EXEC SP_EXECUTESQL @nsql, N'@recordCount INT OUTPUT', @recordCount OUTPUT;
  IF @@ERROR <> 0
  BEGIN
    RETURN -2;
  END

  -- �ж��ܼ�¼��
  IF (@recordCount = 0)
  BEGIN
    SET @pageCount = 0;
    EXEC ('
      SELECT ' + @fields + '
        FROM ' + @tables + '
          WHERE 1=2');
    IF @@ERROR = 0
    BEGIN
      RETURN 0;
    END
    ELSE
    BEGIN
      RETURN -3;
    END
  END

  -- compute page count
  SET @pageCount = CEILING(@recordCount * 1.0 / @pageSize);
  
  -- ��ǰҳ�������ҳ������ѷ�ҳ����Ϊ��ҳ������ʾȡ���һҳ
  IF (@pageIndex > (@pageCount - 1) AND @pageCount > 0)
  BEGIN
    SET @pageIndex = @pageCount - 1;
  END

  -- ����䣨����ǵ�һҳ�����ִ�У�
  IF (@pageIndex = 0)
  BEGIN
    SET @sql = '
      SELECT TOP ' + STR(@pageSize, 3) + ' ' + @fields + '
        FROM ' + @tables;
    IF (@where > '')
    BEGIN
      SET @sql = @sql + '
        WHERE ' + @where;
    END
  END
  ELSE
  BEGIN -- @pageIndex > 0
    -- when target data rownum less than equal to 1000, fill data to temporary, select MAX value
    IF ((@pageIndex + 1) * @pageSize <= 1000)
    BEGIN
      SET @sql = '
        SELECT ' + @fields + '
          FROM (
            SELECT TOP ' + STR((@pageIndex + 1) * @pageSize) + '
                ROW_NUMBER() OVER(ORDER BY ' + @orderField + ' ' + @orderType + ') AS RowID,
                ' + @fields + '
              FROM ' + @tables;
      IF (@where > '') -- �ж��Ƿ�������ֵ
      BEGIN
        SET @sql = @sql + '
              WHERE ' + @where + ') AS tmp';
      END
      ELSE
      BEGIN
        SET @sql = @sql + ') AS tmp';
      END      
      SET @sql = @sql + '
          WHERE tmp.RowID > ' + STR(@pageIndex * @pageSize) + '
          ORDER BY ' + @orderField + ' ' + @orderType;
    END
    ELSE  
    BEGIN -- target data rownum great than 1000 ������ʱ������SQL���
      IF (@orderType = 'ASC')
      BEGIN -- @orderType = 'ASC'
        SET @sql = '
          DECLARE @max VARCHAR(30);
          SELECT @max = MAX(' + @orderField + ')
            FROM (
              SELECT TOP ' + STR(@pageIndex * @pageSize) + ' ' + @orderField + '
                FROM ' + @tables;

        -- �ж��Ƿ�������ֵ
        IF (@where > '')
        BEGIN
          SET @sql = @sql + '
                WHERE ' + @where + '
                ORDER BY ' + @orderField + ' ASC) AS tmp;
           SELECT TOP ' + STR(@pageSize, 3) + '
               ' + @fields + '
             FROM ' + @tables + '
             WHERE ' + @where + '
               AND ' + @orderField + ' > @max
             ORDER BY ' + @orderField + ' ASC;';
        END
        ELSE
        BEGIN
          SET @sql = @sql + '
                ORDER BY ' + @orderField + ' ASC) AS tmp;
           SELECT TOP ' + STR(@pageSize, 3) + '
               ' + @fields + '
             FROM ' + @tables + '
             WHERE ' + @orderField + ' > @max
             ORDER BY ' + @orderField + ' ASC;';
        END
      END -- @orderType = 'ASC'
      ELSE
      BEGIN -- @orderType = 'DESC'
        SET @sql = '
          DECLARE @min VARCHAR(30);
          SELECT @min = MIN(' + @orderField + ')
            FROM (
              SELECT TOP ' + STR(@pageIndex * @pageSize) + ' ' + @orderField + '
                FROM ' + @tables;

        -- �ж��Ƿ�������ֵ
        IF (@where > '')
        BEGIN
          SET @sql = @sql + '
                WHERE ' + @where + '
                ORDER BY ' + @orderField + ' DESC) AS tmp;
           SELECT TOP ' + STR(@pageSize, 3) + '
               ' + @fields + '
             FROM ' + @tables + '
             WHERE ' + @where + '
               AND ' + @orderField + ' < @min
             ORDER BY ' + @orderField + ' DESC;';
        END
        ELSE
        BEGIN
          SET @sql = @sql + '
                ORDER BY ' + @orderField + ' DESC) AS tmp;
           SELECT TOP ' + STR(@pageSize, 3) + '
               ' + @fields + '
             FROM ' + @tables + '
             WHERE ' + @orderField + ' < @min
             ORDER BY ' + @orderField + ' DESC;';
        END        
      END -- @orderType = 'DESC'
    END -- target data great than 1000 ������ʱ������SQL���   
  END -- @pageIndex > 0

  EXEC (@sql);

  IF @@ERROR = 0
  BEGIN
    RETURN 0;
  END
  ELSE
  BEGIN
    RETURN -4;
  END
END
