using Grand.Domain.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Grand.Services.Messages.Tests
{
    [TestClass()]
    public class TokenizerTests {
        [TestMethod()]
        public void Can_replace_tokens_case_sensitive() {
            //method is case sensitive
            MessageTemplatesSettings messageTemplatesSettings = new MessageTemplatesSettings {
                CaseInvariantReplacement = false }; //here I set to BE case sensitive

            Tokenizer tokenizer = new Tokenizer(messageTemplatesSettings);

            List<Token> tokens = new List<Token> { new Token("TokenKey11", "TokenValue11") };

            Assert.AreEqual("some text TokenValue11", tokenizer.Replace("some text %TokenKey11%", tokens, false));
            Assert.AreNotEqual("some text TokenValue11", tokenizer.Replace("some text %tOkEnkEy11%", tokens, false)); //case sensitive
        }

        public void Can_replace_tokens_case_invariant() {
            //method is NOT case sensitive
            MessageTemplatesSettings messageTemplatesSettings = new MessageTemplatesSettings {
                CaseInvariantReplacement = true }; //here I set to NOT BE case sensitive

            Tokenizer tokenizer = new Tokenizer(messageTemplatesSettings);

            List<Token> tokens = new List<Token> { new Token("TokenKey11", "TokenValue11") };

            Assert.AreEqual("some text TokenValue11", tokenizer.Replace("some text %TokenKey11%", tokens, false));
            Assert.AreEqual("some text TokenValue11", tokenizer.Replace("some text %tOkEnkEy11%", tokens, false)); //case insensitive
        }

        public void Can_html_encode() {
            //change < into &lt; 
            MessageTemplatesSettings messageTemplatesSettings = new MessageTemplatesSettings {
                CaseInvariantReplacement = false }; 

            Tokenizer tokenizer = new Tokenizer(messageTemplatesSettings);

            List<Token> tokens = new List<Token> { new Token("TokenKey11", "<TokenValue11>") };

            //html encode set to false (the same as above)
            Assert.AreEqual("some text <TokenValue11>", tokenizer.Replace("some text %TokenKey11%", tokens, false));
            //html encode set to true - it will replace < and > into &lt; and &gt;
            Assert.AreEqual("some text &lt;TokenValue11&gt;", tokenizer.Replace("some text %TokenKey11%", tokens, true));
        }

        public void Should_not_html_encode_if_token_doesnt_allow_it() {
            //see third argument of Token() (Ctor)
            MessageTemplatesSettings messageTemplatesSettings = new MessageTemplatesSettings {
                CaseInvariantReplacement = false
            };

            Tokenizer tokenizer = new Tokenizer(messageTemplatesSettings);

            List<Token> tokens = new List<Token> { new Token("TokenKey11", "<TokenValue11>",true) };
            
            //html encode set to true - BUT IT WON'T replace < and > 
            Assert.AreEqual("some text <TokenValue11>", tokenizer.Replace("some text %TokenKey11%", tokens, true));
        }
    }
}