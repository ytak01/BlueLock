namespace MMFrame.Windows.WindowMessaging
{
    /// <summary>
    /// ウィンドウメッセージに関するクラス
    /// </summary>
    public class WindowMessage
    {
        /// <summary>
        /// P/Invoke
        /// </summary>
        private class NativeMethods
        {
            /// <summary>
            /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
            /// </summary>
            /// <param name="hWnd">ウィンドウのハンドル</param>
            /// <param name="Msg">送信するメッセージ</param>
            /// <param name="wParam">メッセージ特有の追加情報</param>
            /// <param name="lParam">メッセージ特有の追加情報</param>
            /// <returns>メッセージ処理の結果</returns>
            [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "SendMessageA", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, int lParam);

            /// <summary>
            /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
            /// </summary>
            /// <param name="hWnd">ウィンドウのハンドル</param>
            /// <param name="Msg">送信するメッセージ</param>
            /// <param name="wParam">メッセージ特有の追加情報</param>
            /// <param name="lParam">メッセージ特有の追加情報</param>
            /// <returns>メッセージ処理の結果</returns>
            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            public static extern int SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, ref COPYDATASTRUCT lParam);

            /// <summary>
            /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
            /// </summary>
            /// <param name="hWnd">ウィンドウのハンドル</param>
            /// <param name="Msg">送信するメッセージ</param>
            /// <param name="wParam">メッセージ特有の追加情報</param>
            /// <param name="lParam">メッセージ特有の追加情報</param>
            /// <returns>メッセージ処理の結果</returns>
            [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "PostMessageA", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern int PostMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, int lParam);
        }

        /// <summary>
        /// WM_COPYDATA メッセージで別のアプリケーションに渡されるデータの構造体
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public struct COPYDATASTRUCT
        {
            /// <summary>
            /// 受信側アプリケーションに渡される値データ（メッセージ）
            /// </summary>
            public System.IntPtr dwData;

            /// <summary>
            /// <see cref="lpData"/> が指すデータのサイズ（lpData.Length * sizeof(char)）
            /// </summary>
            public int cbData;

            /// <summary>
            /// 受信側アプリケーションに渡されるデータ
            /// </summary>
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpData;
        }

        /// <summary>
        /// 定義済みメッセージの列挙型
        /// </summary>
        public enum Messages
        {
            WM_COPYDATA = 0x004A,
            WM_USER = 0x0400,
            WM_APP = 0x8000
        }

        /// <summary>
        /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
        /// </summary>
        /// <param name="hWnd">メッセージ送信先のウインドウハンドル</param>
        /// <param name="Msg">送信するメッセージ</param>
        /// <param name="wParam">メッセージ送信元のウインドウハンドル</param>
        /// <param name="lParam">送信するデータ</param>
        /// <returns>メッセージ処理の結果</returns>
        public static int SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, int lParam)
        {
            return NativeMethods.SendMessage(hWnd, Msg, wParam, lParam);
        }

        /// <summary>
        /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
        /// </summary>
        /// <param name="hWnd">メッセージ送信先のウインドウハンドル</param>
        /// <param name="Msg">送信するメッセージ</param>
        /// <param name="wParam">メッセージ送信元のウインドウハンドル</param>
        /// <param name="lParam">送信するデータ</param>
        /// <returns>メッセージ処理の結果</returns>
        public static int SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, ref COPYDATASTRUCT lParam)
        {
            return NativeMethods.SendMessage(hWnd, Msg, wParam, ref lParam);
        }

        /// <summary>
        /// 1 つまたは複数のウィンドウへ、指定されたメッセージを送信します。
        /// </summary>
        /// <param name="hWnd">メッセージ送信先のウインドウハンドル</param>
        /// <param name="Msg">送信するメッセージ</param>
        /// <param name="wParam">メッセージ送信元のウインドウハンドル</param>
        /// <param name="lParam">送信するデータ</param>
        /// <returns>メッセージ処理の結果</returns>
        public static int PostMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, int lParam)
        {
            return NativeMethods.PostMessage(hWnd, Msg, wParam, lParam);
        }

        /// <summary>
        /// <see cref="MMFrame.Windows.WindowMessaging.WindowMessageReceiver"/> オブジェクトを取得します。
        /// </summary>
        public WindowMessageReceiver Receiver
        {
            get
            {
                if (!this.IsReceiving)
                {
                    CreateReceiver();
                }

                return this.ReceiverInstance;
            }
        }

        /// <summary>
        /// <see cref="MMFrame.Windows.WindowMessaging.WindowMessageReceiver"/> のハンドルを取得します。
        /// </summary>
        public System.IntPtr ReceiverHandle
        {
            get
            {
                return this.Receiver.Handle;
            }
        }

        /// <summary>
        /// ウィンドウメッセージのレシーバーが起動中かどうかを取得します。
        /// </summary>
        public bool IsReceiving
        {
            get;
            private set;
        }

        /// <summary>
        /// ロックオブジェクトを取得、設定します。
        /// </summary>
        private object LockObject;

        /// <summary>
        /// インスタンス保存用
        /// </summary>
        private WindowMessageReceiver ReceiverInstance;

        /// <summary>
        /// ウィンドウメッセージのレシーバーが使用するスレッド
        /// </summary>
        private System.Threading.Thread ReceiverThread;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WindowMessage()
        {
            this.LockObject = new object();
            this.IsReceiving = false;
        }

        /// <summary>
        /// ウィンドウメッセージのレシーバーを作成します。
        /// </summary>
        public void CreateReceiver()
        {
            lock (this.LockObject)
            {
                if (!this.IsReceiving)
                {
                    using (System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false))
                    {
                        this.ReceiverThread = new System.Threading.Thread(() =>
                        {
                            this.ReceiverInstance = new WindowMessageReceiver();
                            mre.Set();
                            System.Windows.Forms.Application.Run();
                        });

                        this.ReceiverThread.Name = "WindowMessageReceiver";
                        this.ReceiverThread.IsBackground = true;
                        this.ReceiverThread.Start();

                        mre.WaitOne();

                        this.IsReceiving = true;
                    }
                }
            }
        }

        /// <summary>
        /// ウィンドウメッセージのレシーバーを破棄します。
        /// </summary>
        public void DestroyReceiver()
        {
            this.Receiver.Dispose();
            this.ReceiverThread.Abort();
            this.IsReceiving = false;
        }

        /// <summary>
        /// 捕捉するメッセージを追加します。
        /// </summary>
        /// <param name="message">捕捉するメッセージ</param>
        public void RegisterMessage(int message)
        {
            this.Receiver.RegisterMessage(message);
        }

        /// <summary>
        /// 登録されているメッセージを削除します。
        /// </summary>
        /// <param name="message">削除するメッセージ</param>
        public void RemoveMessage(int message)
        {
            this.Receiver.RemoveMessage(message);
        }

        /// <summary>
        /// 登録されているメッセージを全て削除します。
        /// </summary>
        public void ClearMessages()
        {
            this.Receiver.ClearMessages();
        }

        /// <summary>
        /// 捕捉対象メッセージ受信時のイベントを登録します。
        /// </summary>
        /// <param name="receivedEvent">メッセージ受信時のイベント</param>
        public void RegisterEvent(System.Action<System.Windows.Forms.Message> receivedEvent)
        {
            this.Receiver.RegisterEvent(receivedEvent);
        }

        /// <summary>
        /// 登録されているイベントを削除します。
        /// </summary>
        /// <param name="receivedEvent">メッセージ受信時のイベント</param>
        public void RemoveEvent(System.Action<System.Windows.Forms.Message> receivedEvent)
        {
            this.Receiver.RemoveEvent(receivedEvent);
        }

        /// <summary>
        /// 登録されているイベントを全て削除します。
        /// </summary>
        public void ClearEvents()
        {
            this.Receiver.ClearEvents();
        }
    }
}
