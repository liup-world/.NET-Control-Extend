using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: TagPrefix("BobSystem.Controls", "mng")]
namespace BobSystem.Controls
{
    [DefaultProperty("AllowPaging"), DefaultEvent("PageIndexChanging")]
    public class RepeaterX
        : Repeater,
        IPostBackContainer,
        IPostBackEventHandler
    {
        #region FIELD

        PagedDataSource pds = new PagedDataSource()
        {
            PageSize = 15
        };

        private int LastItemIndex = -1;

        private RepeaterFooterStyle footerStyle = new RepeaterFooterStyle();

        #endregion

        [Category("分页"), Description("是否允许分页显示")]
        public bool AllowPaging
        {
            get { return pds.AllowPaging; }
            set { pds.AllowPaging = value; }
        }

        [Category("分页"), Description("获取或设置Repeater每页多少条记录")]
        public int PageSize
        {
            get { return pds.PageSize; }
            set { pds.PageSize = value; }
        }

        [Category("分页"), DefaultValue(0), Description("获取或设置Repeater当前显示页的索引")]
        public int PageIndex
        {
            get { return pds.CurrentPageIndex; }
            set { pds.CurrentPageIndex = value; }
        }

        [Category("分页"), DefaultValue(false), Description("是否使用web.config中AppSetting设置，Key键必须为PageSize")]
        public bool PageSizeUseSetting
        {
            get;
            set;
        }

        [Category("行为"), Description("在无数据时显示的文本")]
        public string EmptyDataText
        {
            get;
            set;
        }

        public override object DataSource
        {
            get { return base.DataSource; }
            set
            {
                #region check value format

                DataTable dt = value as DataTable;
                if (dt != null)
                {
                    value = dt.DefaultView;
                }
                //else
                //{
                //    DataView dv = value as DataView;
                //    if (dv == null)
                //    {
                //        throw new Exception("Assigned data format is not support!");
                //    }
                //}
                #endregion

                string prefix = getVariablePrefix();

                if (AllowPaging)
                {
                    this.pds.DataSource = (IEnumerable)value;
                    #region pds.PageSize = (!PageSizeUseSetting ? PageSize : AppSettings["PageSize"]);

                    if (PageSizeUseSetting)
                    {
                        try
                        {
                            pds.PageSize = int.Parse(ConfigurationManager.AppSettings["PageSize"]);
                        }
                        catch
                        { }
                    }
                    #endregion
                    base.DataSource = pds;

                    ViewState[string.Format("{0}DataCount", prefix)] = pds.DataSourceCount;  // 共有多少记录
                    ViewState[string.Format("{0}CurrIndex", prefix)] = pds.CurrentPageIndex; // 当前页的索引
                    ViewState[string.Format("{0}PageCount", prefix)] = pds.PageCount; // 有多少页
                    ViewState[string.Format("{0}PageSize", prefix)] = pds.PageSize;   // 每一页显示多少记录

                    ViewState[string.Format("{0}IsFirstPage", prefix)] = pds.IsFirstPage;
                    ViewState[string.Format("{0}IsLastPage", prefix)] = pds.IsLastPage;

                    ViewState[string.Format("{0}CurrDataCount", prefix)] = pds.Count; // 实际显示了多少记录
                }
                else
                {
                    base.DataSource = value;

                    ViewState[string.Format("{0}CurrDataCount", prefix)] = ((DataView)value).Count;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.Attribute)]
        [Category("外观"), Description("Repeater模版中FooterTemplate的外观设置")]
        public RepeaterFooterStyle FooterStyle
        {
            get { return footerStyle; }
        }

        protected override void OnItemCreated(RepeaterItemEventArgs e)
        {
            string prefix = getVariablePrefix(); // repeatX_rptCustomer_CurrDataCount

            int currDataCount = (int)ViewState[string.Format("{0}CurrDataCount", prefix)];

            if (e.Item.ItemType != ListItemType.Footer)
            {
                base.OnItemCreated(e);

                this.LastItemIndex = e.Item.ItemIndex;

                #region No Data

                if (e.Item.ItemType == ListItemType.Header && currDataCount == 0)
                {
                    e.Item.Controls.Add(new LiteralControl(string.Format(@"
<tr style=""color:#444; font-size:14px; background-color:white; text-align:left; line-height:22px;"">
  <td colspan=""200"">{0}</td></tr>",
                        string.IsNullOrEmpty(EmptyDataText) ? "没有找到数据。" : EmptyDataText)));

                    AllowPaging = false;
                }
                #endregion
                return;
            }
            if (!AllowPaging)
            {
                return;
            }
            // 效果： 共180条记录，第1页/共36页，每页15条记录   首页 上一页 ... 1 2 3 4 [5] 6 7 8 9 ... 下一页 末页 转到

            e.Item.Controls.Clear();

            int dataCount = (int)ViewState[string.Format("{0}DataCount", prefix)];
            int currIndex = (int)ViewState[string.Format("{0}CurrIndex", prefix)];
            int pageCount = (int)ViewState[string.Format("{0}PageCount", prefix)];
            int pageSize = (int)ViewState[string.Format("{0}PageSize", prefix)];

            bool isFirstPage = (bool)ViewState[string.Format("{0}IsFirstPage", prefix)];
            bool isLastPage = (bool)ViewState[string.Format("{0}IsLastPage", prefix)];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"
<tr style='background-color:white; font-size:12px'>
  <td colspan='{0}'>
    <div class='{1}'>", this.FooterStyle.Colspan, this.FooterStyle.TemplateCssClass);

            #region Record Info

            if (this.FooterStyle.ShowRecordInfo)
            {
                sb.AppendFormat(@"
<span style='width:{0}%; line-height:1.8; text-align:left; float:left; vertical-align:bottom;'>&nbsp;&nbsp;
  共<font style='{1}'>{2}</font>条记录，
  第<font style='{3}'>{4}</font>页/
  共<font style='{5}'>{6}</font>页，
  每页<font style='{7}'>{8}</font>条记录
</span>", (FooterStyle.PagingMode == PagerMode.NextPrevAndNumeric ? 30 : 50),
        FooterStyle.RecordInfoStyle, dataCount,
        FooterStyle.RecordInfoStyle, currIndex + 1,
        FooterStyle.RecordInfoStyle, pageCount,
        FooterStyle.RecordInfoStyle, pageSize);
            }
            #endregion

            sb.AppendFormat("<span class='{0}' style='float:right; {1}'>\n", this.FooterStyle.PagerCssClass,
                (!FooterStyle.ShowRecordInfo ?
                string.Format("width:100%; text-align:center; {0}", this.FooterStyle.PagerStyle) :
                this.FooterStyle.PagerStyle));

            e.Item.Controls.Add(new LiteralControl(sb.ToString()));

            #region First Prev

            if (FooterStyle.PagingMode == PagerMode.PrevNext || FooterStyle.PagingMode == PagerMode.NextPrevAndNumeric)
            {
                if (!isFirstPage)
                {
                    e.Item.Controls.Add(createButton(PagerType.First, 0));
                    e.Item.Controls.Add(new LiteralControl("&nbsp;&nbsp;\n"));

                    e.Item.Controls.Add(createButton(PagerType.Previous, currIndex - 1));
                    e.Item.Controls.Add(new LiteralControl("&nbsp;&nbsp;\n"));
                }
                else
                {
                    e.Item.Controls.Add(new LiteralControl("首页&nbsp;&nbsp;上一页&nbsp;&nbsp;\n"));
                }
            }
            #endregion

            #region Numeric Field

            // 9 10 11 [12] 13 14 15
            if (FooterStyle.PagingMode == PagerMode.Numeric || FooterStyle.PagingMode == PagerMode.NextPrevAndNumeric)
            {
                int numCount = this.FooterStyle.NumberCount;

                if (base.DesignMode)
                {
                    pageCount = numCount;
                }

                int half = numCount / 2;
                int start = currIndex - half;
                if (start < 0)
                {
                    start = 0;
                }
                int end = start + numCount;
                if (end > pageCount - 1)
                {
                    end = pageCount;
                    if (end > numCount)
                    {
                        start = end - numCount;
                    }
                }
                for (; start < end; ++start)
                {
                    if (start != currIndex)
                    {
                        e.Item.Controls.Add(createButton(PagerType.Numeric, start));
                        e.Item.Controls.Add(new LiteralControl("&nbsp;\n"));
                    }
                    else
                    {
                        e.Item.Controls.Add(new LiteralControl(string.Format("[{0}]&nbsp;&nbsp;", start + 1)));
                    }
                }
                e.Item.Controls.Add(new LiteralControl("&nbsp;"));
            }
            #endregion

            #region Next Last

            if (FooterStyle.PagingMode == PagerMode.PrevNext || FooterStyle.PagingMode == PagerMode.NextPrevAndNumeric)
            {
                if (!isLastPage)
                {
                    e.Item.Controls.Add(createButton(PagerType.Next, currIndex + 1));
                    e.Item.Controls.Add(new LiteralControl("&nbsp;&nbsp;\n"));

                    e.Item.Controls.Add(createButton(PagerType.Last, pageCount - 1));
                    e.Item.Controls.Add(new LiteralControl("&nbsp;&nbsp;&nbsp;"));
                }
                else
                {
                    e.Item.Controls.Add(new LiteralControl("下一页&nbsp;&nbsp;末页&nbsp;&nbsp;&nbsp\n"));
                }
            }
            #endregion

            e.Item.Controls.Add(new LiteralControl("\n转到第"));

            #region TextBox and Button

            //Regex regex = new Regex(@"(// *.*)*\s+"); // replace white space in foo

            int itemIndex = LastItemIndex + 2;
            string itemID = itemIndex < 100 ? itemIndex.ToString("00") : itemIndex.ToString("000");
            TextBox txtPageNo = new TextBox();
            txtPageNo.ID = "txtGo";
            txtPageNo.Text = (currIndex + 1).ToString();
            txtPageNo.Attributes.Add("style", "width:18px");
            txtPageNo.Attributes.Add("maxlength", "3");

            // ON KEY DOWN
            txtPageNo.Attributes.Add("onkeydown",
                string.Format("repeatX_txtPageNo_onkeydown(this, '{0}', '{1}')", base.ClientID, itemID));

            // ON CHANGE
            txtPageNo.Attributes.Add("onchange",
                string.Format("repeatX_txtPageNo_onchange(this, '{0}', '{1}')", base.ClientID, itemID));

            // ON PASTE
            txtPageNo.Attributes.Add("onpaste", "javascript:return false;"); //repeatX_txtPageNo_onpaste()

            // ON DRAGENTER
            txtPageNo.Attributes.Add("ondragenter", "javascript:return false;");

            // ON FOCUS
            txtPageNo.Attributes.Add("onfocus", "repeatX_txtPageNo_onfocus(this)");

            e.Item.Controls.Add(txtPageNo);
            e.Item.Controls.Add(new LiteralControl("页\n"));

            RepeaterXPagerLinkButton btnGo = new RepeaterXPagerLinkButton(this);
            btnGo.ID = "btnGo";
            btnGo.Text = "Go&nbsp;&nbsp;";
            btnGo.Attributes.Add("href",
                string.Format("javascript:__doPostBack('{0}', 'go${1}')", base.ClientID, currIndex + 1));
            btnGo.Attributes.Add("style", "color:royalblue; text-decoration:none;");
            btnGo.CommandName = "go";

            // ON CLICK
            btnGo.OnClientClick = string.Format("repeatX_btnGo_onclick('{0}', '{1}')", base.ClientID, itemID);

            e.Item.Controls.Add(btnGo);

            #endregion

            e.Item.Controls.Add(new LiteralControl("&nbsp;&nbsp;\n</span></div>\n</td></tr></table>"));

            base.OnItemCreated(e);

            // link javascript file
            base.Page.ClientScript.RegisterClientScriptResource(GetType(), "BobSystem.Controls.Scripts.repeatX_footer_required.js");
        }

        private RepeaterXPagerLinkButton createButton(PagerType pagerType, int pageIndex)
        {
            RepeaterXPagerLinkButton btn = new RepeaterXPagerLinkButton(this);

            switch (pagerType)
            {
                case PagerType.First:
                    btn.Text = "首页";
                    btn.Font.Underline = false;
                    break;
                case PagerType.Previous:
                    btn.Text = "上一页";
                    btn.Font.Underline = false;
                    break;
                case PagerType.Next:
                    btn.Text = "下一页";
                    btn.Font.Underline = false;
                    break;
                case PagerType.Last:
                    btn.Text = "末页";
                    btn.Font.Underline = false;
                    break;
                case PagerType.Numeric:
                    btn.Text = (pageIndex + 1).ToString();
                    break;
                case PagerType.NumericPrevNext:
                    btn.Text = "...";
                    break;
            }
            btn.CommandName = "navigate";
            btn.CommandArgument = pageIndex.ToString();

            return btn;
        }

        // delegate
        public delegate void RepeaterPageEventHandle(object sender, RepeaterPageEventArgs e);

        [Description("Repeater当前页索引更改时时执行的事件")]
        public event RepeaterPageEventHandle PageIndexChanging;

        protected void OnPageIndexChanging(RepeaterPageEventArgs e)
        {
            if (e.Cancel) return;

            if (PageIndexChanging == null)
            {
                throw new NotImplementedException("Event \"PageIndexChanging\" not implemented or assign.");
            }

            PageIndexChanging(this, e);
        }

        #region IMPLEMENT IPostBackEventHandler

        public void RaisePostBackEvent(string param)
        {
            string[] arr = param.Split('$');
            string cmd = arr[0];
            string index = arr[1];

            if (cmd == "navigate")
            {
                OnPageIndexChanging(new RepeaterPageEventArgs(int.Parse(index)));
                return;
            }

            try
            {
                int tmp = 0;
                if (int.TryParse(index, out tmp))
                {
                    tmp -= 1;
                }

                object currIndex = ViewState[string.Format("{0}CurrIndex", getVariablePrefix())];
                if (currIndex != null && tmp != (int)currIndex)
                {
                    OnPageIndexChanging(new RepeaterPageEventArgs(validateIndex(tmp)));
                }
            }
            catch
            { }
        }

        #endregion

        #region IMPLEMENT IPostBackContainer

        public PostBackOptions GetPostBackOptions(IButtonControl btn)
        {
            if (btn == null) throw new ArgumentNullException("navigate button of repeaterX is null.");

            if (btn.CausesValidation) throw new InvalidOperationException();

            return new PostBackOptions(this, string.Format("{0}${1}", btn.CommandName, btn.CommandArgument))
            {
                RequiresJavaScriptProtocol = true
            };
        }

        #endregion

        private int validateIndex(int index)
        {
            if (index <= 0) return 0;

            object pageCount = ViewState[string.Format("{0}PageCount", getVariablePrefix())];
            if (pageCount != null && index >= (int)pageCount)
            {
                return (int)pageCount - 1;
            }

            return index;
        }

        private string getVariablePrefix()
        {
            return string.Format("repeaterX_{0}_", base.ClientID);
        }

        //  navigate button class
        internal class RepeaterXPagerLinkButton
            : LinkButton
        {
            private IPostBackContainer container;
            internal RepeaterXPagerLinkButton(IPostBackContainer container)
            {
                this.container = container;
            }

            protected override PostBackOptions GetPostBackOptions()
            {
                if (container != null)
                {
                    return container.GetPostBackOptions(this);
                }

                return base.GetPostBackOptions();
            }

            public override bool CausesValidation
            {
                get { return false; }
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
        public class RepeaterFooterStyle
        {
            #region FIELD

            private string templateCssClass = "footer-template";

            private bool showRecordInfo = true;

            private string recordInfoStyle = "color:red;";

            private PagerMode pagerMode = PagerMode.NextPrevAndNumeric;

            private string pagerCssClass = "pager";

            private string pagerStyle = string.Empty;

            private int numberCount = 7;

            private int colspan = 50;

            #endregion

            [DefaultValue("footer-template"), Description("脚模版CSS样式名称"), NotifyParentProperty(true)]
            public string TemplateCssClass
            {
                get { return templateCssClass; }
                set { templateCssClass = value; }
            }

            [DefaultValue(true), Description("指示在FootTemplate是否显示Repeater绑定数据的信息"), NotifyParentProperty(true)]
            public bool ShowRecordInfo
            {
                get { return showRecordInfo; }
                set { showRecordInfo = value; }
            }

            [DefaultValue("color:red;"), Description("绑定数据信息的关键数字的Style样式"), NotifyParentProperty(true)]
            public string RecordInfoStyle
            {
                get { return recordInfoStyle; }
                set { recordInfoStyle = value; }
            }

            [DefaultValue(PagerMode.NextPrevAndNumeric), Description("Repeate分页方式，显示的元素"), NotifyParentProperty(true)]
            public PagerMode PagingMode
            {
                get { return pagerMode; }
                set { pagerMode = value; }
            }

            [DefaultValue("pager"), Description("分页导航CSS样式名称"), NotifyParentProperty(true)]
            public string PagerCssClass
            {
                get { return pagerCssClass; }
                set { pagerCssClass = value; }
            }

            [DefaultValue(""), Description("分页导航Style样式"), NotifyParentProperty(true)]
            public string PagerStyle
            {
                get { return pagerStyle; }
                set { pagerStyle = value; }
            }

            [DefaultValue(7), Description("显示数字页码的个数"), NotifyParentProperty(true)]
            public int NumberCount
            {
                get { return numberCount; }
                set { numberCount = value; }
            }

            [DefaultValue(50), Description("脚模版合并的列数（绑定数据列数）"), NotifyParentProperty(true)]
            public int Colspan
            {
                get { return colspan; }
                set { colspan = value; }
            }
        }
    }

    /// <summary>
    /// Repeater分页事件参数类型
    /// </summary>
    public class RepeaterPageEventArgs
    {
        public RepeaterPageEventArgs()
            : this(0)
        { }

        public RepeaterPageEventArgs(int newPageIndex)
            : this(newPageIndex, false)
        { }

        public RepeaterPageEventArgs(int newPageIndex, bool cancle)
        {
            this.NewPageIndex = newPageIndex;
            this.Cancel = cancle;
        }

        [Description("要显示的页面的索引"), DefaultValue(0)]
        public int NewPageIndex
        {
            get;
            set;
        }

        [Description("指示是否取消执行事件"), DefaultValue(false)]
        public bool Cancel
        {
            get;
            set;
        }
    }
}

/// <summary>
/// 指定分页元素
/// </summary>
public enum PagerMode
{
    /// <summary>
    /// 首页 上一页 下一页 末页
    /// </summary>
    PrevNext,
    /// <summary>
    /// 数字： 1 2 [3] 4 5
    /// </summary>
    Numeric,
    /// <summary>
    /// 首页 上一页  1 2 [3] 4 5  下一页 末页
    /// </summary>
    NextPrevAndNumeric
}
