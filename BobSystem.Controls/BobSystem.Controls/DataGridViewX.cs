
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace BobSystem.Controls
{
    public partial class DataGridViewX
        : DataGridView
    {
        #region FIELD

        private Panel pnlContainer = null;

        private Label lblInfo = null;


        private bool allowPaging = false;

        private int pageIndex = 0;

        private int pageSize = 15;

        private int pagingNumericCount = 7;

        private int PageCount;

        private int RecordCount;

        private bool showRecordInfo = true;

        private PagingProcdureObject pagingProcedure = new PagingProcdureObject();

        private PagerMode pagerMode = PagerMode.NextPrevAndNumeric;

        private ButtonTemplate buttonTemplate = new ButtonTemplate();

        private int TotalWidth = 0;

        #endregion

        #region AllowPaging

        [Category("分页"), DefaultValue(false), Description("是否允许分页显示")]
        public bool AllowPaging
        {
            get { return this.allowPaging; }
            set
            {
                this.allowPaging = value;
                if (this.pnlContainer != null)
                {
                    this.pnlContainer.Visible = value;
                }
                if (value)
                {
                    dataBind();
                }
            }
        }

        #endregion

        #region PageIndex

        [Category("分页"), DefaultValue(0), Description("获取或设置DataGridViewX当前显示页的索引")]
        public int PageIndex
        {
            get { return pageIndex; }
            set { pageIndex = value; }
        }

        #endregion

        #region PageSize

        [Category("分页"), DefaultValue(15), Description("获取或设置DataGridViewX每页多少条记录")]
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }

        #endregion

        #region PagingNumericCount

        [Category("分页"), DefaultValue(7), Description("获取或设置DataGridViewX数字导航的个数")]
        public int PagingNumericCount
        {
            get { return this.pagingNumericCount; }
            set { this.pagingNumericCount = value; }
        }

        #endregion

        #region PagingProcedure

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("分页"), Description("获取或设置DataGridViewX分页存储过程信息")]
        public PagingProcdureObject PagingProcedure
        {
            get { return this.pagingProcedure; }
        }

        #endregion

        #region PagingMode

        [Category("分页"), DefaultValue(PagerMode.NextPrevAndNumeric), Description("DataGridView分页方式，显示的元素")]
        public PagerMode PagingMode
        {
            get { return pagerMode; }
            set { pagerMode = value; }
        }

        #endregion

        #region ShowRecordInfo

        [Category("外观"), DefaultValue(true), Description("指示是否显示的绑定数据信息"), NotifyParentProperty(true)]
        public bool ShowRecordInfo
        {
            get { return showRecordInfo; }
            set { showRecordInfo = value; }
        }

        #endregion

        #region PagingButtonTemplate

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("外观"), Description("设置此按钮的某些属性，可将这些属性应用到DataGridViewX的导航按钮上")]
        public ButtonTemplate PagingButtonTemplate
        {
            get { return buttonTemplate; }
        }

        #endregion

        [Browsable(false)]
        public new BorderStyle BorderStyle
        {
            get;
            set;
        }

        // CREATE CHILDREN CONTROLS
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!this.allowPaging)
            {
                return;
            }

            dataBind();
        }

        private void dataBind()
        {
            if (base.DesignMode)
            {
                createPagingControl();
                return;
            }

            string connStr = getConnectionString();
            if (string.IsNullOrEmpty(connStr))
            {
                return;
            }

            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(this.PagingProcedure.Name, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(new SqlParameter[]
            {
                #region PRAMETERS & RETURN VALUE

                new SqlParameter()
                {
                    ParameterName = "@tables",
                    Value = this.pagingProcedure.TablesList,
                    DbType = DbType.String,
                },
                new SqlParameter()
                {
                    ParameterName = "@fields",
                    Value = this.pagingProcedure.Fields,
                    DbType = DbType.String,
                },
                new SqlParameter()
                {
                    ParameterName = "@orderField",
                    Value = this.pagingProcedure.OrderField,
                    DbType = DbType.String,
                },
                new SqlParameter()
                {
                    ParameterName = "@pageSize",
                    Value = this.PageSize,
                    DbType = DbType.Int32,
                },
                new SqlParameter()
                {
                    ParameterName = "@pageIndex",
                    Value = this.PageIndex,
                    DbType = DbType.Int32,
                },
                new SqlParameter()
                {
                    ParameterName = "@where",
                    Value = this.pagingProcedure.Condition,
                    DbType = DbType.String,
                },
                new SqlParameter()
                {
                    ParameterName = "@orderType",
                    Value = (this.pagingProcedure.OrderType == PagingProcdureObject.DataOrderType.ASC ? "ASC" : "DESC"),
                    DbType = DbType.String,
                },
                new SqlParameter()
                {
                    ParameterName = "@pageCount",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output,
                },
                new SqlParameter()
                {
                    ParameterName = "@recordCount",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output,
                },
                new SqlParameter()
                {
                    ParameterName = "@returnCode",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.ReturnValue,
                },
                #endregion
            });
            try
            {
                conn.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet set = new DataSet();
                adapter.Fill(set);

                int returnCode = (int)cmd.Parameters["@returnCode"].Value;
                if (returnCode == 0)
                {
                    this.RecordCount = (int)cmd.Parameters["@recordCount"].Value;
                    this.PageCount = (int)cmd.Parameters["@pageCount"].Value;

                    if (set == null || set.Tables.Count == 0) return;

                    DataTable dataTable = set.Tables[0];

                    base.DataSource = dataTable;

                    //
                    createPagingControl();

                    this.lblInfo.Text = string.Empty;
                }
                else
                {
                    switch (returnCode)
                    {
                        case -1:
                            this.lblInfo.Text = "表名列表为空。";
                            break;
                        case -2:
                            this.lblInfo.Text = "获取总记录数量出现错误。";
                            break;
                        case -3:
                        default:
                            this.lblInfo.Text = "未知错误。";
                            break;
                        case -4:
                            this.lblInfo.Text = "执行查询出现错误。";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                createInfoLabel();

                this.lblInfo.Text = ex.Message;
            }
            cmd.Parameters.Clear();
            cmd.Dispose();
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        private string getConnectionString()
        {
            if (!string.IsNullOrEmpty(this.pagingProcedure.ConnString))
            {
                if (this.pagingProcedure.ConnStringUseSetting)
                {
                    return ConfigurationManager.ConnectionStrings[this.pagingProcedure.ConnString].ConnectionString;
                }
            }

            return this.pagingProcedure.ConnString;
        }

        private void createPagingControl()
        {
            if (this.pnlContainer == null)
            {
                #region PANEL CONTAINER

                this.pnlContainer = new Panel();
                this.pnlContainer.Left = 1;
                this.pnlContainer.Top = this.Size.Height - 51;
                this.pnlContainer.Width = this.Width - 2;
                this.pnlContainer.Height = 50;
                this.pnlContainer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                this.pnlContainer.SizeChanged += new EventHandler(panelContainer_SizeChanged);
                this.pnlContainer.Paint += new PaintEventHandler(panelContainer_Paint);

                base.Controls.Add(this.pnlContainer);

                #endregion

                // LABEL INFO
                createInfoLabel();
            }

            this.pnlContainer.Controls.Clear();

            int containerWidth = this.pnlContainer.Width;
            int containerHeight = this.pnlContainer.Height;

            int left = 0;
            int top = 0;
            if (this.ShowRecordInfo)
            {
                top = (containerHeight - this.buttonTemplate.Height) / 4;
            }
            else
            {
                top = (containerHeight - this.buttonTemplate.Height) / 2;
            }
            int spacing = 10;

            if (PagingMode == PagerMode.PrevNext || PagingMode == PagerMode.NextPrevAndNumeric)
            {
                #region First Previous

                // first
                Button btnFirst = buttonTemplate.Clone();
                btnFirst.Text = "首页";
                btnFirst.Location = new Point(left, top);
                btnFirst.Click += new EventHandler(btnFirst_Click);

                left += btnFirst.Width + spacing;

                // previous
                Button btnPrevious = buttonTemplate.Clone();
                btnPrevious.Text = "上一页";
                btnPrevious.Location = new Point(left, top);
                btnPrevious.Click += new EventHandler(btnPrevious_Click);

                left += btnPrevious.Width + spacing;

                #endregion

                this.pnlContainer.Controls.Add(btnFirst);
                this.pnlContainer.Controls.Add(btnPrevious);

                if (pageIndex == 0)
                {
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                }
            }
            // 9 10 11 [12] 13 14 15
            if (PagingMode == PagerMode.Numeric || PagingMode == PagerMode.NextPrevAndNumeric)
            {
                #region Numeric Field

                int numCount = this.PagingNumericCount;

                if (base.DesignMode)
                {
                    this.PageCount = numCount;
                }

                int half = numCount / 2;
                int start = this.pageIndex - half;
                if (start < 0)
                {
                    start = 0;
                }
                int end = start + numCount;
                if (end > this.PageCount - 1)
                {
                    end = this.PageCount;
                    if (end > numCount)
                    {
                        start = end - numCount;
                    }
                }
                for (; start < end; ++start)
                {
                    LinkLabel linkButton = new LinkLabel();
                    linkButton.Text = (start + 1).ToString();
                    linkButton.Tag = start;
                    linkButton.Width = linkButton.Text.Length * 9;
                    linkButton.Location = new Point(left, top + 5);
                    linkButton.Click += new EventHandler(pagingNumber_Click);

                    if (start == this.pageIndex)
                    {
                        linkButton.Enabled = false;
                    }

                    this.pnlContainer.Controls.Add(linkButton);

                    left += linkButton.Width;
                    if (start <= 99)
                    {
                        left += spacing;
                    }
                }

                #endregion
            }
            if (PagingMode == PagerMode.PrevNext || PagingMode == PagerMode.NextPrevAndNumeric)
            {
                #region Next Last

                // next
                Button btnNext = buttonTemplate.Clone();
                btnNext.Text = "下一页";
                btnNext.Location = new Point(left, top);
                btnNext.Click += new EventHandler(btnNext_Click);

                left += btnNext.Width + spacing;

                // last
                Button btnLast = buttonTemplate.Clone();
                btnLast.Text = "末页";
                btnLast.Location = new Point(left, top);
                btnLast.Click += new EventHandler(btnLast_Click);

                left += btnLast.Width + spacing;

                #endregion

                this.pnlContainer.Controls.Add(btnNext);
                this.pnlContainer.Controls.Add(btnLast);

                if (pageIndex == this.PageCount - 1)
                {
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                }
            }
            #region TextBox Button

            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = "跳转至第";
            lbl.Location = new Point(left, top + 5);

            left += 53;

            TextBox txtPageNo = new TextBox();
            txtPageNo.Text = (this.PageIndex + 1).ToString();
            txtPageNo.Width = 24;
            txtPageNo.MaxLength = 5;
            txtPageNo.BorderStyle = BorderStyle.FixedSingle;
            txtPageNo.TextAlign = HorizontalAlignment.Right;
            txtPageNo.AllowDrop = false;
            txtPageNo.ShortcutsEnabled = false;
            txtPageNo.Location = new Point(left, top);
            txtPageNo.KeyPress += new KeyPressEventHandler(txtPageNo_KeyPress);

            left += txtPageNo.Width + 2;

            Button btnGo = this.buttonTemplate.Clone();
            btnGo.Text = "Go";
            btnGo.Width = 30;
            btnGo.Tag = txtPageNo;
            btnGo.Location = new Point(left, top);
            btnGo.Click += new EventHandler(btnGo_Click);

            left += txtPageNo.Width + 30;

            this.pnlContainer.Controls.Add(lbl);
            this.pnlContainer.Controls.Add(txtPageNo);
            this.pnlContainer.Controls.Add(btnGo);

            #endregion

            #region Record Info

            if (this.ShowRecordInfo)
            {
                top += this.buttonTemplate.Height;

                Label lblRecordInfo = new Label();
                lblRecordInfo.AutoSize = true;
                lblRecordInfo.Top = top + 5;
                lblRecordInfo.Text = string.Format("共[{0}]条记录，第[{1}]页/共[{2}]页，每页[{3}]条记录",
                    this.RecordCount, this.PageIndex + 1,
                    this.PageCount, this.PageSize);

                this.pnlContainer.Controls.Add(lblRecordInfo);

                lblRecordInfo.Left = (containerWidth - lblRecordInfo.Width) / 2;
            }
            #endregion

            // 将生成的控件移至中间
            this.TotalWidth = left;

            adaptContainerSize();

            this.pnlContainer.Top = this.ClientSize.Height - 51;
            if (base.HorizontalScrollBar.Visible)
            {
                this.pnlContainer.Top -= this.HorizontalScrollBar.Height;
            }
            if (base.VerticalScrollBar.Visible)
            {
                base.VerticalScrollBar.Maximum += 200;
                base.VerticalScrollBar.Value = base.VerticalScrollBar.Maximum;
            }
        }

        private void createInfoLabel()
        {
            if (this.lblInfo == null)
            {
                this.lblInfo = new Label()
                {
                    AutoSize = true,
                    Top = this.Size.Height - 15,
                    ForeColor = Color.Red,
                };

                base.Controls.Add(this.lblInfo);
            }
        }

        private void adaptContainerSize()
        {
            int oldLeft = this.pnlContainer.Controls[0].Left;

            int excursion = (this.pnlContainer.Width - this.TotalWidth) / 2 - oldLeft;

            foreach (Control ctrl in this.pnlContainer.Controls)
            {
                ctrl.Left += excursion;
            }

            if (this.ShowRecordInfo)
            {
                Label lblRecordInfo = (Label)this.pnlContainer.Controls[this.pnlContainer.Controls.Count - 1];
                lblRecordInfo.Left = (this.pnlContainer.Width - lblRecordInfo.Width) / 2;
            }
        }

        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(brush);

            e.Graphics.DrawLine(pen, 0, 0, this.pnlContainer.Width - 1, 0);
            //e.Graphics.DrawLine(pen, 0, 0, 0, this.panel.Height - 1);
            //e.Graphics.DrawLine(pen, this.panel.Width - 1, 0, this.panel.Width - 1, this.panel.Height - 1);
        }

        protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            base.OnColumnWidthChanged(e);

            this.pnlContainer.Top = this.ClientSize.Height - 51;
            if (base.HorizontalScrollBar.Visible)
            {
                this.pnlContainer.Top -= this.HorizontalScrollBar.Height;
            }
        }

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);

            SolidBrush brush = new SolidBrush(base.RowHeadersDefaultCellStyle.ForeColor);

            e.Graphics.DrawString((e.RowIndex + 1).ToString(), // System.Globalization.CultureInfo.CurrentUICulture
                base.DefaultCellStyle.Font, brush, e.RowBounds.X + 10, e.RowBounds.Y + 5);
        }

        #region Children Control Event

        // first previous next last Button Click Event
        private void btnFirst_Click(object sender, EventArgs e)
        {
            this.pageIndex = 0;

            dataBind();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (this.pageIndex != 0)
            {
                this.pageIndex = this.pageIndex - 1;

                dataBind();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.pageIndex != this.PageCount - 1)
            {
                this.pageIndex = this.pageIndex + 1;

                dataBind();
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            this.pageIndex = this.PageCount - 1;

            dataBind();
        }

        // pagingNumber_Click
        private void pagingNumber_Click(object sender, EventArgs e)
        {
            int index = (int)((Control)sender).Tag;

            this.pageIndex = index;

            dataBind();
        }

        // panelContainer_SizeChanged
        private void panelContainer_SizeChanged(object sender, EventArgs e)
        {
            adaptContainerSize();
        }

        // txtPageNo_KeyPress
        private void txtPageNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 48 && e.KeyChar <= 57) || // 0-9
                //(event.keyCode >= 96 && event.keyCode <= 105) || 
                e.KeyChar == 8 || // 退格
                //e.KeyChar == 46 || // '.'
                e.KeyChar == 27 || // ESC
                e.KeyChar == 37 || // 左
                e.KeyChar == 39 || // 右
                e.KeyChar == 16 || // shift
                e.KeyChar == 9))  // Tab
            {
                e.Handled = true;
            }
            if (e.KeyChar == 13)
            {
                TextBox txtPageNo = (TextBox)sender;
                int pageNo = int.Parse(txtPageNo.Text);

                this.PageIndex = pageNo - 1;

                dataBind();
            }
        }

        // btnGo_Click
        private void btnGo_Click(object sender, EventArgs e)
        {
            TextBox txtPageNo = (TextBox)((Button)sender).Tag;
            int pageNo = int.Parse(txtPageNo.Text);

            if (this.PageIndex == pageNo - 1) return;

            this.PageIndex = pageNo - 1;

            dataBind();
        }
        
        #endregion

        //public static explicit operator DataGridView(DataGridViewX seft)
        //{
        //    return seft.dataGridView;
        //}

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PagingProcdureObject
        {
            #region FIELD

            string name = "PagingProcedure";

            string tablesList = string.Empty;
            string fields = "*";
            string condition = string.Empty;
            string orderField = "ID";
            DataOrderType orderType = DataOrderType.ASC;

            #endregion

            [DefaultValue("PagingProcedure"), NotifyParentProperty(true)]
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            [Description("表名，支持多表联合查询，逗号分隔，指定条件要正确"), NotifyParentProperty(true)]
            public string TablesList
            {
                get { return tablesList; }
                set { tablesList = value; }
            }

            [DefaultValue("*"), Description("查询字段名，逗号分隔，至少要包括“排序字段”"), NotifyParentProperty(true)]
            public string Fields
            {
                get { return fields; }
                set { fields = value; }
            }

            [Description("where条件，不加WHERE"), NotifyParentProperty(true)]
            public string Condition
            {
                get { return condition; }
                set { condition = value; }
            }

            [DefaultValue("ID"), Description("排序字段"), NotifyParentProperty(true)]
            public string OrderField
            {
                get { return orderField; }
                set { orderField = value; }
            }

            [DefaultValue(DataOrderType.ASC), Description("排序类型，ASC/DESC"), NotifyParentProperty(true)]
            public DataOrderType OrderType
            {
                get { return orderType; }
                set { orderType = value; }
            }

            [NotifyParentProperty(true)]
            [DefaultValue(false), Description("是否使用.config中<connectionStrings></connectionStrings>节点的配置")]
            public bool ConnStringUseSetting
            {
                get;
                set;
            }

            [Description("数据库链接字符串或者<connectinStrings>中的名字"), NotifyParentProperty(true)]
            public string ConnString
            {
                get;
                set;
            }

            public enum DataOrderType
            {
                ASC,
                DESC,
            }
        }

        /// <summary>
        /// 导航元素的分类
        /// </summary>
        enum PagerType
        {
            /// <summary>
            /// 首页
            /// </summary>
            First,
            /// <summary>
            /// 上一页
            /// </summary>
            Previous,
            /// <summary>
            /// 下一页
            /// </summary>
            Next,
            /// <summary>
            /// 末页
            /// </summary>
            Last,
            /// <summary>
            /// 数字导航元素
            /// </summary>
            Numeric,
            /// <summary>
            /// 数字导航元素的 头 尾
            /// </summary>
            NumericPrevNext
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class ButtonTemplate
            : Button
        {
            public Button Clone()
            {
                Button btn = new Button();
                btn.BackColor = this.BackColor;
                btn.BackgroundImage = this.BackgroundImage;
                btn.BackgroundImageLayout = this.BackgroundImageLayout;
                btn.Bounds = this.Bounds;
                btn.Cursor = this.Cursor;
                btn.Dock = this.Dock;
                btn.FlatStyle = this.FlatStyle;
                btn.FlatAppearance.BorderColor = this.FlatAppearance.BorderColor;
                btn.FlatAppearance.BorderSize = this.FlatAppearance.BorderSize;
                btn.FlatAppearance.CheckedBackColor = this.FlatAppearance.CheckedBackColor;
                btn.FlatAppearance.MouseDownBackColor = this.FlatAppearance.MouseDownBackColor;
                btn.FlatAppearance.MouseOverBackColor = this.FlatAppearance.MouseOverBackColor;
                btn.Font = this.Font;
                btn.ForeColor = this.ForeColor;
                btn.Height = this.Height;
                btn.Image = this.Image;
                btn.ImageAlign = this.ImageAlign;
                btn.Margin = this.Margin;
                btn.Padding = this.Padding;
                btn.Size = this.Size;
                btn.TextAlign = this.TextAlign;
                btn.TextImageRelation = this.TextImageRelation;
                btn.Width = this.Width;
                btn.Visible = this.Visible;

                return btn;
            }
        }
    }
}
