using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pageflex.Interfaces.Storefront;
using PageflexServices;
using System.Web;
using System.Web.UI;

namespace moveAllToHold
{
    public class moveAllToHold: SXIExtension
    {
        public override string UniqueName
        {
            get
            {
                return "MoveAllToHold.standardgroup.com";
            }
        }
        public override string DisplayName
        {
            get
            {
                return "TSG: Move All To Hold";
            }
        }

        public override int GetConfigurationHtml(KeyValuePair[] parameters, out string HTML_configString)
        {
            string movesel = base.Storefront.GetValue("ModuleField", "ExtensionMoveAll", "MoveAllToHold.standardgroup.com");
            string delsel = base.Storefront.GetValue("ModuleField", "ExtensionDeleteAll", "MoveAllToHold.standardgroup.com");
            string first = "<br><br><strong>Extension Configuration:</strong><br><br><table><tr><td>Move Selected(Y or N):</td><td><input type='text' size='10' name='movall' value='" + movesel + "'>";
            string second = "</td></tr><tr><td>Delete Selected(Y or N):</td><td><input type='text' size='10' name='delall' value='" + delsel + "'>";
            string end = "</td></tr></table>";
            HTML_configString = null;
            if (parameters == null)
            {
                HTML_configString = first+second+end;
            }
            else
            {
                bool isMoveBad = false;
                bool isDelBad = false;
                foreach (KeyValuePair pair in parameters)
                {
                    if (pair.Name.Equals("movall") && (pair.Value.Length == 0))
                    {
                        isMoveBad = true;
                        //HTML_configString = first + "<font color=red><strong>Required.</strong></font>";
                    }
                    if (pair.Name.Equals("movall") && (pair.Value.Length != 0))
                    {
                        base.Storefront.SetValue("ModuleField", "ExtensionMoveAll", "MoveAllToHold.standardgroup.com", pair.Value);
                    }
                    if (pair.Name.Equals("delall") && (pair.Value.Length == 0))
                    {
                        isDelBad = true;
                        //HTML_configString = second + "<font color=red><strong>Required.</strong></font>";
                    }
                    if (pair.Name.Equals("delall") && (pair.Value.Length != 0))
                    {
                        base.Storefront.SetValue("ModuleField", "ExtensionDeleteAll", "MoveAllToHold.standardgroup.com", pair.Value);
                    }
                }
                string firstReq = "";
                string secondReq = "";
                if (isMoveBad) {
                    firstReq = "<font color=red><strong>Required.</strong></font>";
                }
                if (isDelBad) {
                    secondReq = "<font color=red><strong>Required.</strong></font>";
                }
                if (isMoveBad || isDelBad)
                    HTML_configString = first + firstReq + second + secondReq + end;
            }
            return 0;
        }



        public override int PageLoad(string pageBaseName, string eventName)
        {
            var page = HttpContext.Current.CurrentHandler as Page;   
            if ((pageBaseName == "usercontentshoppingcart_aspx" ) && (eventName == "Init"))
            {
                string url = HttpContext.Current.Request.Url.AbsoluteUri;
                string[] findVars = url.Split('?');
                
                string userId = Storefront.GetValue("SystemProperty", "LoggedOnUserID", null);
                string[] docInCart = Storefront.GetListValue("UserListProperty", "DocumentsInShoppingCart", userId);
                string[] docOnHold = Storefront.GetListValue("UserListProperty", "DocumentsOnHold", userId);
                //string check = Storefront.GetValue("DocumentProperty", "ExternalID", docInCart[0]).Replace("-", "");
                string orderId = Storefront.GetValue("SystemProperty","PendingOrder",userId);
                //string onHold = "OnHold";
                if (findVars.Length > 1){
                    string[] moveMe = findVars[2].Split(',');
                    
                    if (findVars[1].Equals("move") || findVars[1].Equals("moveH") || findVars[1].Equals("del") || findVars[1].Equals("delH"))
                    {
                        int[] idxOfMove = new int[moveMe.Length - 1];
                        
                        for (int i = 0; i < moveMe.Length - 1; i++) {
                            
                            bool found = false;
                            string docName = "";
                            if (findVars[1].Equals("move") || findVars[1].Equals("del"))
                            {
                                for (int c = 0; c < docInCart.Length && !found; c++)
                                {
                                    docName = Storefront.GetValue("DocumentProperty", "ExternalID", docInCart[c]).Replace("-", "");
                                    if (docName.Equals(moveMe[i]))
                                    {
                                        found = true;
                                        idxOfMove[i] = c;
                                    }
                                }
                            }
                            else if (findVars[1].Equals("moveH") || findVars[1].Equals("delH"))
                            {
                                for (int c = 0; c < docOnHold.Length && !found; c++)
                                {
                                    docName = Storefront.GetValue("DocumentProperty", "ExternalID", docOnHold[c]).Replace("-", "");
                                    if (docName.Equals(moveMe[i]))
                                    {
                                        found = true;
                                        idxOfMove[i] = c;
                                    }
                                }
                            }
                                
                        }
                        for (int i = 0; i < idxOfMove.Length; i++)
                        {
                            if (findVars[1].Equals("move"))
                                Storefront.SetValue("DocumentProperty", "EditingStatus", docInCart[idxOfMove[i]], "OnHold");
                            else if (findVars[1].Equals("moveH"))
                                Storefront.SetValue("DocumentProperty", "EditingStatus", docOnHold[idxOfMove[i]], "InCart");
                            else if (findVars[1].Equals("del"))
                                Storefront.DeleteDocument(docInCart[idxOfMove[i]]);
                            else if (findVars[1].Equals("delH"))
                                Storefront.DeleteDocument(docOnHold[idxOfMove[i]]);
                        }
                        HttpContext.Current.Response.Redirect(findVars[0]);
                    }
                }

                string js = createJavaScript(userId);
                page.ClientScript.RegisterStartupScript(this.GetType(), "Add_Check_Box", js);
                    
            }

            

            
            return eSuccess;
        }

        

        private string createJavaScript(string userId) {
            string movesel = base.Storefront.GetValue("ModuleField", "ExtensionMoveAll", "MoveAllToHold.standardgroup.com");
            string delsel = base.Storefront.GetValue("ModuleField", "ExtensionDeleteAll", "MoveAllToHold.standardgroup.com");
            string js = "<script type='text/javascript' src='jSINI.js'></script>";
            if (movesel == "Y" || delsel == "Y")
            {
                js += "<style type = 'text/css'>";
                js += "a.disabledForButtons {";
                js += " opacity: 0.5;";
                js += " pointer-events: none;";
                js += " cursor: default;";
                js += "}";
                js += "</style>";
                js += "<script type = 'text/javascript'>";
                js += "Sys.WebForms.PageRequestManager.getInstance().add_endRequest(myJQueryRequestHandler);";
                js += "function myJQueryRequestHandler(sender,args){";
                js += "addColToCart();}";
                js += "$(document).ready(function(){";
                //js += "alert('" + docName + "');";
                js += "addColToCart();";
                js += "});";
                //adds checkboxes to the Incart and On Hold shopping carts 
                js += "function addColToCart(){";
                js += "$('.itemTableHeader-Thumbnail').before('<td class=\"itemTableHeader itemTableHeader-Id\"><input type=\"checkbox\" id=\"checkHead\" name=\"SelectAll\" value=\"move\"></td>');";
                //js += "$('.itemTable-Id').hide();";
                js += "$('#CartTable .itemTable-Id').each(function(i) {";
                //js += "     alert($(this).text());";
                js += "     var id = $(this).text().replace(/‑/, \"\");";
                js += "     id = id.replace(\"‑\",\"\");";
                //js += "     alert(id);";
                js += "     $(this).parent().closest('.itemTable-Thumbnail').before('<td class=\"itemTable itemTable-Check\"><input type=\"checkbox\" class=\"aBox\" name=\"select\" value='+id+'></td>');});";
                js += "$('#HoldTable .itemTable-Id').each(function(i) {";
                js += "     var id = $(this).text().replace(/‑/, \"\");";
                js += "     id = id.replace(\"‑\",\"\");";
                //js += "     var intId = CallSINIMethod('FindDocumentID', [id]);";
                js += "     $(this).parent().closest('.itemTable-Thumbnail').before('<td class=\"itemTable itemTable-Check\"><input type=\"checkbox\" class=\"aBox\" name=\"select\" value='+id+'></td>');});";
                //js += "$('#ShoppingCart_CartPanel').append('<div><button type=\"button\" name=\"SelAll\" value=\"all\" onclick=checkConfirm() >Move Selected To Hold</button></div>');";
                js += "var appendMeCart = '';";
                js += "var appendMeHold = '';";
                if (movesel == "Y")
                {
                    if (delsel == "Y"){
                        js += "appendMeCart += '<table width=\"50%\"><tr>';";
                        js += "appendMeHold += '<table width=\"50%\"><tr>';";
                    }
                    else{
                        js += "appendMeCart += '<table width=\"30%\"><tr>';";
                        js += "appendMeHold += '<table width=\"30%\"><tr>';";
                    }
                    js += " appendMeCart += '<td width=\"50%\">";
                    js += " <div class=\"siteButton\" style = \"pull: left; text-align: center;margin-top: 10px; margin-bottom: 0px\"><div class=\"siteButton-t\"><div class=\"siteButton-b\">";
                    js += " <div class=\"siteButton-l\"><div class=\"siteButton-r\"><div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
                    js += " <div class=\"siteButton-inner\"><a id=\"checkMeCart\" class=\"siteButton\" href=\"javascript:checkConfirm(moveToHold)\">";
                    js += " Move Selected To Hold</a></div></div></div></div></div></div></div></div></div></div></td>';";

                    js += " appendMeHold += '<td width=\"50%\">";
                    
                    js += "<div class=\"siteButton\" style = \"pull: left; text-align: center;margin-top: 10px; margin-bottom: 0px\"><div class=\"siteButton-t\"><div class=\"siteButton-b\">";
                    js += " <div class=\"siteButton-l\"><div class=\"siteButton-r\"><div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
                    js += " <div class=\"siteButton-inner\"><a id=\"checkMeHold\" class=\"siteButton\" href=\"javascript:checkConfirm(moveToCart)\">";
                    js += " Move Selected To Cart</a></div></div></div></div></div></div></div></div></div></div></td>';";
                }
                if (delsel == "Y")
                {
                    js += " appendMeCart += '<td width=\"50%\">";
                    
                    js += " <div class=\"siteButton\" style = \" text-align: center;margin-top: 10px; margin-bottom: 0px; margin-left: 10px;\"><div class=\"siteButton-t\"><div class=\"siteButton-b\">";
                    js += " <div class=\"siteButton-l\"><div class=\"siteButton-r\"><div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
                    js += " <div class=\"siteButton-inner\"><a id=\"checkMeCartDel\" class=\"siteButton\" href=\"javascript:checkConfirm(deleteFileFromCart)\">";
                    js += " Delete Selected</a></div></div></div></div></div></div></div></div></div></div></td></tr>';";

                    js += " appendMeHold += '<td width=\"50%\">";

                    js += "<div class=\"siteButton\" style = \"text-align: center;margin-top: 10px; margin-bottom: 0px; margin-left: 10px;\"><div class=\"siteButton-t\"><div class=\"siteButton-b\">";
                    js += " <div class=\"siteButton-l\"><div class=\"siteButton-r\"><div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
                    js += " <div class=\"siteButton-inner\"><a id=\"checkMeHoldDel\" class=\"siteButton\" href=\"javascript:checkConfirm(deleteFileFromHold)\">";
                    js += " Delete Selected</a></div></div></div></div></div></div></div></div></div></div></td>';";
                }
                js += "appendMeCart += '</table>';";
                js += "appendMeHold += '</table>';";
                js += "$('#ShoppingCart_CartPanel').append(appendMeCart);";
                js += "$('#HoldCart_CartPanel').append(appendMeHold);";
                js += "checkIfCartChecked();checkIfHoldChecked();";
                
                js += "$('#CartTable input:checkbox').click( function(e) {";
                js += "         if(this.name == 'SelectAll'){";
                js += "             if($('#CartTable #checkHead').get(0).checked){";
                js += "                 $('#CartTable .aBox').attr('checked', true);}else{$('#CartTable .aBox').attr('checked', false);}}";
                js += "         else{$('#CartTable #checkHead').get(0).checked = false;}";
                js += "checkIfCartChecked();});";
                js += "$('#HoldTable input:checkbox').click( function(e) {";
                js += "         if(this.name == 'SelectAll'){";
                js += "             if($('#HoldTable #checkHead').get(0).checked){";
                js += "                 $('#HoldTable .aBox').attr('checked', true);}else{$('#HoldTable .aBox').attr('checked', false);}}";
                js += "         else{$('#HoldTable #checkHead').get(0).checked = false}";
                js += "checkIfHoldChecked();});}";
                js += "function moveToHold(){";
                //js += "     var myList = CallSINIMethod('GetListValue', ['UserListProperty','DocumentsInShoppingCart'," + userId + "]);";
                js += "     var i = 0;";
                js += "     var loc = document.location.toString();";
                js += "     loc += '?move?';";
                //js += "     while(i < myList.length){";
                //js += "         CallSINIMethod('SetValue',['VariableValue', 'ShoppingCartStatus', myList[i], 'MoveToHold']);";
                //js += "         i+=1;}";
                js += "     var boxes =('#CartTable .aBox:checked');";
                js += "     $(boxes).each(function(i){loc+=this.value;loc+=',';});";
                js += "     loc+= ' ';";
                js += "     document.location=loc;";
                js += "}";
              
                js += "function checkConfirm(myFunction){";
                js += "     if(confirm('Are You Sure?'))";
                js += "     myFunction();";
                js += "}";
                
                js += "function deleteFileFromCart(){";
                //js += "     var myList = CallSINIMethod('GetListValue', ['UserListProperty','DocumentsInShoppingCart'," + userId + "]);";
                js += "     var i = 0;";
                js += "     var loc = document.location.toString();";
                js += "     loc += '?del?';";
                js += "     var boxes =$('#CartTable .aBox:checked');";
                js += "     $(boxes).each(function(i){loc+=this.value;loc+=',';});";
                js += "     loc+= ' ';";
                js += "     document.location=loc;";
                js += "}";
                js += "function deleteFileFromHold(){";
                //js += "     var myList = CallSINIMethod('GetListValue', ['UserListProperty','DocumentsOnHold'," + userId + "]);";
                js += "     var i = 0;";
                js += "     var loc = document.location.toString();";
                js += "     loc += '?delH?';";
                js += "     var boxes =$('#HoldTable .aBox:checked');";
                js += "     $(boxes).each(function(i){loc+=this.value;loc+=',';});";
                js += "     loc+= ' ';";
                js += "     document.location=loc;";
                js += "}";
                js += "function moveToCart(){";
                //js += "     var myList = CallSINIMethod('GetListValue', ['UserListProperty','DocumentsOnHold'," + userId + "]);";
                js += "     var i = 0;";
                js += "     var loc = document.location.toString();";
                js += "     loc += '?moveH?';";
                //js += "     while(i < myList.length){";
                //js += "         CallSINIMethod('SetValue',['VariableValue', 'ShoppingCartStatus', myList[i], 'MoveToHold']);";
                //js += "         i+=1;}";
                js += "     var boxes =$('#HoldTable .aBox:checked');";
                js += "     $(boxes).each(function(i){loc+=this.value;loc+=',';});";
                js += "     loc+= ' ';";
                js += "     document.location=loc;";
                js += "}";
                js += "function checkIfCartChecked(){";
                js += "     var boxes =$('#CartTable .aBox:checked');";
                js += "     if(boxes.length <= 0){";
                js += "         $('#checkMeCart').addClass('disabledForButtons');";
                js += "         $('#checkMeCartDel').addClass('disabledForButtons');";
                js += "         $('#checkMeCart').bind('click', function(e){";
                js += "             e.preventDefault();";
                js += "         });";
                js += "         $('#checkMeCartDel').bind('click', function(e){";
                js += "             e.preventDefault();";
                js += "         });";
                js += "     }else{";
                js += "         $('#checkMeCart').removeClass('disabledForButtons');";
                js += "         $('#checkMeCartDel').removeClass('disabledForButtons');";
                js += "         $('#checkMeCart').unbind('click');";
                js += "         $('#checkMeCartDel').unbind('click');";
                js += "}}";
                js += "function checkIfHoldChecked(){";
                js += "     var boxes =$('#HoldTable .aBox:checked');";
                js += "     if(boxes.length <= 0){";
                js += "         $('#checkMeHold').addClass('disabledForButtons');";
                js += "         $('#checkMeHoldDel').addClass('disabledForButtons');";
                js += "         $('#checkMeHold').bind('click', function(e){";
                js += "             e.preventDefault();";
                js += "         });";
                js += "         $('#checkMeHoldDel').bind('click', function(e){";
                js += "             e.preventDefault();";
                js += "         });";
                js += "     }else{";
                js += "         $('#checkMeHold').removeClass('disabledForButtons');";
                js += "         $('#checkMeHoldDel').removeClass('disabledForButtons');";
                js += "         $('#checkMeHold').unbind('click');";
                js += "         $('#checkMeHoldDel').unbind('click');";
                js += "}}";
            }
            js += "</script>";
            return js;
        }
    }
}
