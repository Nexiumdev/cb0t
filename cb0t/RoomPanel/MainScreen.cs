﻿using Awesomium.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace cb0t
{
    class MainScreen : WebControl
    {
        public MainScreen(IContainer c) : base(c) { }

        public void CreateScreen()
        {
            this.ViewIdent = -1;
            this.LoadingFrameComplete += this.ScreenIsReady;
            this.ShowContextMenu += this.DefaultContextMenu;
            String template = Helpers.ScreenHTML;
            template = template.Replace("[font]", Settings.GetReg<String>("global_font", "Tahoma"));
            template = template.Replace("[size]", Settings.GetReg<int>("global_font_size", 10) + "pt;");

            if (this.IsBlack)
                template = template.Replace("FFFFFF", "000000");

            base.LoadHTML(template);
            
        }

        private void DefaultContextMenu(object sender, Awesomium.Core.ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        public void OnLinkClicked(System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<System.Windows.Forms.LinkClickedEventArgs>(this.OnLinkClicked), e);
            else
            {
                if (e.LinkText.StartsWith("http://hashlink.script/?h="))
                {
                    String str = e.LinkText.Substring(26);

                    if (!String.IsNullOrEmpty(str))
                        Scripting.ScriptManager.InstallScript(str);
                }
                else if (e.LinkText.StartsWith("http://hashlink.link/?h="))
                {
                    DecryptedHashlink hashlink = Hashlink.DecodeHashlink(e.LinkText.Substring(24));

                    if (hashlink != null)
                        this.HashlinkClicked(hashlink, EventArgs.Empty);
                }
                else
                {
                    String check = e.LinkText.ToUpper();

                    if (check.StartsWith("HTTP://") || check.StartsWith("HTTPS://") || check.StartsWith("WWW."))
                    {
                        Scripting.JSOutboundTextItem cb = new Scripting.JSOutboundTextItem();
                        cb.Type = Scripting.JSOutboundTextItemType.Link;
                        cb.Text = e.LinkText;
                        cb.EndPoint = this.EndPoint;
                        Scripting.ScriptManager.PendingUIText.Enqueue(cb);
                    }
                }
            }
        }

        private bool IsScreenReady { get; set; }
        public int ViewIdent { get; set; }

        private void ScreenIsReady(object sender, Awesomium.Core.FrameEventArgs e)
        {
            this.LoadingFrameComplete -= this.ScreenIsReady;
            this.IsScreenReady = true;
            this.ViewIdent = this.Identifier;
        }

        public event EventHandler HashlinkClicked;
        public event EventHandler NameClicked;
        public event EventHandler EditScribbleClicked;

        public bool IsMainScreen { get; set; }
        public bool IsBlack { get; set; }
        public IPEndPoint EndPoint { get; set; }
        
        private int cls_count = 0;

        public void UpdateTemplate()
        {

        }

        public void ScrollDown()
        {

        }

        public void Scribble(byte[] data)
        {

        }

        public void ShowAnnounceText(String text)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<String>(this.ShowAnnounceText), text);
            else
            {
                /*if (this.IsPaused)
                {
                    this.paused_items.Add(new PausedItem { Type = PausedItemType.Announce, Text = text });
                    return;
                }*/

                if (text.Replace("\n", "").Replace("\r", "").Length == 0)
                {
                    if (this.cls_count++ > 6)
                        if (Settings.GetReg<bool>("block_cls", false))
                            return;

                    if (this.cls_count > 200)
                        return;

                    if (text.Count(x => x == '\r' || x == '\n') > 20)
                        if (Settings.GetReg<bool>("block_cls", false))
                            return;
                }
                else if (Helpers.StripColors(text).Length <= 2)
                {
                    if (this.cls_count++ > 6)
                        if (Settings.GetReg<bool>("block_cls", false))
                            return;

                    if (this.cls_count > 200)
                        return;
                }
                else this.cls_count = 0;

                bool ts = Settings.GetReg<bool>("can_timestamp", false);
                this.Render(ts ? (Helpers.Timestamp + text) : text, null, true, 4, null);
            }
        }

        public void ShowServerText(String text)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<String>(this.ShowServerText), text);
            else
            {
             /*   if (this.IsPaused)
                {
                    this.paused_items.Add(new PausedItem { Type = PausedItemType.Server, Text = text });
                    return;
                } */

                this.cls_count = 0;
                bool ts = Settings.GetReg<bool>("can_timestamp", false);
                this.Render(ts ? (Helpers.Timestamp + text) : text, null, true, this.IsBlack ? 15 : 2, null);
            }
        }

        public void ShowPublicText(String name, String text, AresFont font)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<String, String, AresFont>(this.ShowPublicText), name, text, font);
            else
            {
                /*if (this.IsPaused)
                {
                    this.paused_items.Add(new PausedItem { Type = PausedItemType.Public, Name = name, Text = text });
                    return;
                }*/

                this.cls_count = 0;
                bool ts = Settings.GetReg<bool>("can_timestamp", false);
                this.Render(text, ts ? (Helpers.Timestamp + name) : name, true, this.IsBlack ? 0 : 12, font);
            }
        }

        public void ShowEmoteText(String name, String text, AresFont font)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<String, String, AresFont>(this.ShowEmoteText), name, text, font);
            else
            {
                /*if (this.IsPaused)
                {
                    this.paused_items.Add(new PausedItem { Type = PausedItemType.Emote, Name = name, Text = text });
                    return;
                }*/

                this.cls_count = 0;
                bool ts = Settings.GetReg<bool>("can_timestamp", false);
                this.Render((ts ? Helpers.Timestamp : "") + "* " + name + " " + text, null, false, this.IsBlack ? 13 : 6, font);
            }
        }

        public bool IsWideText { get; set; }

        public void ShowCustomHTML(String html)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action<String>(this.ShowCustomHTML), html);
            else
                base.ExecuteJavascript("injectCustomHTML(\"" + html.Replace("\"", "\\\"") + "\")");
        }

        private void Render(String txt, String name, bool can_col, int first_col, AresFont _ff)
        {
            if (!this.IsScreenReady)
                return;

            String text = txt.Replace("\r\n", "\r").Replace("\n",
                "\r").Replace("", "").Replace("]̽", "").Replace(" ̽",
                "").Replace("͊", "").Replace("]͊", "").Replace("͠",
                "").Replace("̶", "").Replace("̅", "");

            StringBuilder html = new StringBuilder();
            String col_tester;
            AresFont ff = _ff;

            if (!Settings.GetReg<bool>("receive_ppl_fonts", true))
                ff = null;

            if (ff != null)
            {
                col_tester = ff.TextColor.ToUpper();

                if (col_tester == "#000000" && this.IsBlack)
                    ff.TextColor = "#FFFFFF";
                else if (col_tester == "#FFFFFF" && !this.IsBlack)
                    ff.TextColor = "#000000";
            }

            bool bold = false,
                 italic = false,
                 underline = false,
                 bg_code_used = false;
            
            bool can_emoticon = Settings.GetReg<bool>("can_emoticon", true);
            int emote_count = 0;
            String fore, back, tmp, link;
            bool can_link = true;
            int itmp;

            if (ff == null)
                fore = AresColorToHTMLColor(first_col);
            else
                fore = ff.TextColor;

            back = this.IsBlack ? "#000000" : "#FFFFFF";
            html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));

            char[] letters = text.ToCharArray();

            for (int i = 0; i < letters.Length; i++)
            {
                switch (letters[i])
                {
                    case '\x0006':
                        bold = !bold;
                        html.Append("</span>");
                        html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                        break;

                    case '\x0007':
                        underline = !underline;
                        html.Append("</span>");
                        html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                        break;

                    case '\x0009':
                        italic = !italic;
                        html.Append("</span>");
                        html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                        break;

                    case '\x0003':
                        if (letters.Length >= (i + 8))
                        {
                            tmp = text.Substring((i + 1), 7);

                            if (Helpers.IsHexCode(tmp))
                            {
                                if (!bg_code_used)
                                {
                                    if (this.IsBlack && tmp == "#000000")
                                        tmp = "#FFFFFF";
                                    else if (!this.IsBlack && tmp.ToUpper() == "#FFFFFF")
                                        tmp = "#000000";
                                }

                                fore = tmp;
                                html.Append("</span>");
                                html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                                i += 7;
                                break;
                            }
                        }

                        if (letters.Length >= (i + 3))
                        {
                            tmp = text.Substring((i + 1), 2);

                            if (int.TryParse(tmp, out itmp))
                            {
                                if (!bg_code_used)
                                {
                                    if (this.IsBlack && itmp == 1)
                                        itmp = 0;
                                    else if (!this.IsBlack && itmp == 0)
                                        itmp = 1;
                                }

                                fore = this.AresColorToHTMLColor(itmp);
                                html.Append("</span>");
                                html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                                i += 2;
                                break;
                            }
                        }
                        else can_link = false;
                        goto default;

                    case '\x0005':
                        if (letters.Length >= (i + 8))
                        {
                            tmp = text.Substring((i + 1), 7);

                            if (Helpers.IsHexCode(tmp))
                            {
                                back = tmp;
                                html.Append("</span>");
                                html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                                bg_code_used = true;
                                i += 7;
                                break;
                            }
                        }

                        if (letters.Length >= (i + 3))
                        {
                            tmp = text.Substring((i + 1), 2);

                            if (int.TryParse(tmp, out itmp))
                            {
                                back = this.AresColorToHTMLColor(itmp);
                                html.Append("</span>");
                                html.Append(this.BuildFontStyle(fore, back, bold, italic, underline));
                                bg_code_used = true;
                                i += 2;
                                break;
                            }
                        }
                        else can_link = false;
                        goto default;

                    case '(':
                    case ':':
                    case ';':
                        can_link = false;

                        if (can_emoticon)
                        {
                            Emotic em = Emoticons.FindEmoticon(text.ToString().Substring(i).ToUpper());

                            if (em != null)
                            {
                                if (emote_count++ < 8)
                                {
                                    html.Append("<img style=\"height: 19px; margin-bottom: -4px;\" alt=\"\" src=\"http://emotic.org/" + em.Index + ".gif\" />");
                                    i += (em.Shortcut.Length - 1);
                                    break;
                                }
                                else goto default;
                            }

                            em = this.FindNewEmoticon(text.ToString().Substring(i).ToUpper());

                            if (em != null)
                            {
                                if (emote_count++ < 8)
                                {
                                    itmp = Emoticons.GetExEmoticonHeight(em.Index);
                                    tmp = em.Shortcut.Substring(1, em.Shortcut.Length - 2).ToLower();
                                    html.Append("<img style=\"height: " + itmp + "px; margin-bottom: -4px;\" alt=\"\" src=\"http://emotic.ext/" + tmp + ".gif\" />");
                                    i += (em.Shortcut.Length - 1);
                                    break;
                                }
                                else goto default;
                            }
                        }
                        goto default;

                    case 'w':
                    case 'h':
                    case 'a':
                    case '\\':
                        tmp = text.Substring(i);

                        if (can_link)
                            if (tmp.IndexOf("http://") == 0 ||
                                tmp.IndexOf("https://") == 0 ||
                                tmp.IndexOf("www.") == 0 ||
                                tmp.IndexOf("arlnk://") == 0 ||
                                tmp.IndexOf("\\\\arlnk://") == 0 ||
                                tmp.IndexOf("\\\\cb0t://script/?file=") == 0)
                            {
                                int _end = tmp.IndexOf(" ");

                                if (_end > -1)
                                    tmp = tmp.Substring(0, _end);

                                i += (tmp.Length - 1);

                                link = "";
                                String pre_link = "";

                                if (tmp.IndexOf("\\\\cb0t://script/?file=") == 0)
                                {
                                    String lnk_text = "";

                                    for (int a = 0; a < tmp.Length; a++)
                                    {
                                        lnk_text += ("&#" + ((int)tmp[a]) + ";");

                                        if (a >= 2)
                                            pre_link += ("&#" + ((int)tmp[a]) + ";");
                                    }

                                    tmp = tmp.Substring(22);

                                    if (tmp.Length > 0)
                                    {
                                        tmp = "http://hashlink.script/?h=" + tmp;

                                        for (int a = 0; a < tmp.Length; a++)
                                            link += ("&#" + ((int)tmp[a]) + ";");

                                        html.Append("<a href=\"" + link + "\" title=\"" + pre_link + "\" style=\"text-decoration: underline; cursor: pointer;\">" + lnk_text + "</a>");
                                        break;
                                    }
                                }
                                else if (tmp.IndexOf("arlnk://") == 0 || tmp.IndexOf("\\\\arlnk://") == 0)
                                {
                                    String hash_str = tmp;

                                    if (hash_str.StartsWith("arlnk://"))
                                        hash_str = hash_str.Substring(8);

                                    if (hash_str.StartsWith("\\\\arlnk://"))
                                        hash_str = hash_str.Substring(10);

                                    DecryptedHashlink dec_hash = Hashlink.DecodeHashlink(hash_str);

                                    if (dec_hash == null)
                                        break;

                                    tmp = "arlnk://" + hash_str;
                                    String hash_title = String.Empty;

                                    for (int a = 0; a < tmp.Length; a++)
                                        hash_title += ("&#" + ((int)tmp[a]) + ";");

                                    tmp = "http://hashlink.link/?h=" + hash_str;
                                    String hash_href = String.Empty;

                                    for (int a = 0; a < tmp.Length; a++)
                                        hash_href += ("&#" + ((int)tmp[a]) + ";");

                                    tmp = StringTemplate.Get(STType.HashlinkSettings, 0) + ": " + dec_hash.Name;
                                    String hash_inner = String.Empty;

                                    for (int a = 0; a < tmp.Length; a++)
                                        hash_inner += ("&#" + ((int)tmp[a]) + ";");

                                    String hash_img = "<img style=\"margin-bottom: -4px; border-style: none; height: 24px;\" src=\"http://emotic.ui/hashlink.png\" alt=\"\" />";
                                    html.Append("<a href=\"" + hash_href + "\" title=\"" + hash_title + "\" style=\"text-decoration: underline; cursor: pointer;\">" + hash_img + " " + hash_inner + "</a>");
                                    break;
                                }
                                else
                                {
                                    if (tmp.IndexOf("www.") == 0)
                                    {
                                        pre_link = "http://";

                                        for (int a = 0; a < pre_link.Length; a++)
                                            link += ("&#" + ((int)pre_link[a]) + ";");

                                        pre_link = link;
                                        link = "";
                                    }

                                    for (int a = 0; a < tmp.Length; a++)
                                        link += ("&#" + ((int)tmp[a]) + ";");

                                    html.Append("<a href=\"" + pre_link + link + "\" title=\"" + link + "\" style=\"text-decoration: underline; cursor: pointer;\">" + link + "</a>");
                                    break;
                                }
                            }

                        can_link = false;
                        html.Append("&#" + ((int)letters[i]) + ";");
                        break;

                    case ' ':
                        can_link = true;
                        html.Append("&#" + ((int)letters[i]) + ";");
                        break;

                    default:
                        can_link = false;
                        html.Append("&#" + ((int)letters[i]) + ";");
                        break;
                }
            }

            html.Append("</span>");

            if (!String.IsNullOrEmpty(name))
            {
                StringBuilder name_builder = new StringBuilder();

                if (ff != null)
                {
                    col_tester = ff.NameColor.ToUpper();

                    if (col_tester == "#000000" && this.IsBlack)
                        ff.NameColor = "#FFFF00";
                    else if (col_tester == "#FFFFFF" && !this.IsBlack)
                        ff.NameColor = "#000000";
                }

                if (ff == null)
                    fore = this.IsBlack ? "#FFFF00" : "#000000";
                else
                    fore = ff.NameColor;

                back = this.IsBlack ? "#000000" : "#FFFFFF";
                name_builder.Append(this.BuildFontStyle(fore, back, false, false, false));

                char[] name_chrs = (name + "> ").ToCharArray();

                for (int i = 0; i < name_chrs.Length; i++)
                    name_builder.Append("&#" + ((int)name_chrs[i]) + ";");

                name_builder.Append("</span>");
                html.Insert(0, name_builder.ToString());
                name_builder.Clear();
            }

            if (ff == null)
            {
                StringBuilder font_style = new StringBuilder();
                font_style.Append("<span style=\"");
                font_style.Append("font-family: " + Settings.GetReg<String>("global_font", "Tahoma") + ";");
                font_style.Append("font-size: " + Settings.GetReg<int>("global_font_size", 10) + "pt;");
                font_style.Append("\">");
                html.Insert(0, font_style.ToString());
                html.Append("</span>");
                font_style.Clear();
            }
            else
            {
                StringBuilder font_style = new StringBuilder();
                font_style.Append("<span style=\"");
                font_style.Append("font-family: " + ff.FontName + ";");
                int org_size = Settings.GetReg<int>("global_font_size", 10);
                int difference = (org_size - 10);
                int user_size = ff.Size + difference;
                font_style.Append("font-size: " + user_size + "pt;");
                font_style.Append("\">");
                html.Insert(0, font_style.ToString());
                html.Append("</span>");
                font_style.Clear();
            }

            base.ExecuteJavascript("injectText(\"" + html.ToString().Replace("\"", "\\\"") + "\", " + (this.IsWideText ? "true" : "false") + ")");
            html.Clear();
        }

        private String AresColorToHTMLColor(int c)
        {
            switch (c)
            {
                case 1: return "#000000";
                case 0: return "#FFFFFF";
                case 8: return "#FFFF00";
                case 11: return "#00FFFF";
                case 12: return "#0000FF";
                case 2: return "#000080";
                case 6: return "#800080";
                case 9: return "#00FF00";
                case 13: return "#FF00FF";
                case 14: return "#808080";
                case 15: return "#C0C0C0";
                case 7: return "#FFA500";
                case 5: return "#800000";
                case 10: return "#008080";
                case 3: return "#008000";
                case 4: return "#FF0000";
                default: return "#FFFFFF";
            }
        }

        private Emotic FindNewEmoticon(String str)
        {
            for (int i = 0; i < Emoticons.ex_emotic.Length; i++)
                if (str.StartsWith("(" + Emoticons.ex_emotic[i].ShortcutText + ")"))
                    return new Emotic { Index = i, Shortcut = "(" + Emoticons.ex_emotic[i].ShortcutText + ")" };

            return null;
        }

        private String BuildFontStyle(String fc, String bc, bool b, bool i, bool u)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<span style=\"");
            sb.Append("color: " + fc + ";");
            sb.Append("background-color: " + bc + ";");

            if (b)
                sb.Append("font-weight: bold;");

            if (i)
                sb.Append("font-style: italic;");

            if (u)
                sb.Append("text-decoration: underline;");

            sb.Append("\">");
            return sb.ToString();
        }

        public void Free()
        {
            this.IsScreenReady = false;
            this.ShowContextMenu -= this.DefaultContextMenu;
        }
    }
}