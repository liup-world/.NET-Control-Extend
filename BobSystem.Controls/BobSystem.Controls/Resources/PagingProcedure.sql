
CREATE PROCEDURE PagingProcedure
(
  @tables      VARCHAR(300),        -- 表名，支持多表联合查询，逗号分隔，指定条件要正确
  @fields      VARCHAR(500) = '*',  -- 查询字段名，逗号分隔
  @orderField  VARCHAR(30)  = 'ID', -- 排序字段
  @pageSize    INT          = 15,   -- 每页记录数，大于0
  @pageIndex   INT          = 0,    -- 当前第几页，0为首页
  @where       VARCHAR(500) = '',   -- 条件，不加WHERE
  @orderType   VARCHAR(4)   = 'ASC', -- 排序类型，ASC/DESC
  @pageCount   INT          = 0 OUTPUT, -- 总页数
  @recordCount INT          = 0 OUTPUT -- 总记录数  
)
AS
BEGIN
  SET NOCOUNT ON;

  -- 参数合法检查
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

  -- 判断总记录数
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
  
  -- 当前页码大于总页数，则把分页码置为总页数，表示取最后一页
  IF (@pageIndex > (@pageCount - 1) AND @pageCount > 0)
  BEGIN
    SET @pageIndex = @pageCount - 1;
  END

  -- 主语句（如果是第一页则加速执行）
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
      IF (@where > '') -- 判断是否有条件值
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
    BEGIN -- target data rownum great than 1000 不用临时表来求SQL语句
      IF (@orderType = 'ASC')
      BEGIN -- @orderType = 'ASC'
        SET @sql = '
          DECLARE @max VARCHAR(30);
          SELECT @max = MAX(' + @orderField + ')
            FROM (
              SELECT TOP ' + STR(@pageIndex * @pageSize) + ' ' + @orderField + '
                FROM ' + @tables;

        -- 判断是否有条件值
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

        -- 判断是否有条件值
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
    END -- target data great than 1000 不用临时表来求SQL语句   
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
