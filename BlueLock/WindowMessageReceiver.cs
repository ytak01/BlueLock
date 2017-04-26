namespace MMFrame.Windows.WindowMessaging
{
    /// <summary>
    /// ウィンドウメッセージのレシーバークラス
    /// </summary>
    public class WindowMessageReceiver : System.Windows.Forms.NativeWindow
    {
        /// <summary>
        /// リソースが解放されているかどうかを取得、設定します。
        /// </summary>
        private bool IsDisposed;

        /// <summary>
        /// 補足するメッセージのリストを取得、設定します。
        /// </summary>
        private System.Collections.Generic.HashSet<int> CatchMessages;

        /// <summary>
        /// 登録イベントのリストを取得、設定します。
        /// </summary>
        private System.Collections.Generic.HashSet<System.Action<System.Windows.Forms.Message>> Events;

        /// <summary>
        /// <see cref="System.Threading.ReaderWriterLock"/> を取得、設定します。
        /// </summary>
        private System.Threading.ReaderWriterLock Lock;

        /// <summary>
        /// <see cref="System.Threading.SynchronizationContext"/> を取得、設定します。
        /// </summary>
        private System.Threading.SynchronizationContext SynchronizationContext;

        /// <summary>
        /// メッセージ補足時のイベントを取得、設定します。
        /// </summary>
        private event System.Action<System.Windows.Forms.Message> ReceivedEvent;

        /// <summary>
        /// <see cref="MMFrame.Windows.WindowMessaging.WindowMessageReceiver"/> オブジェクトを生成します。
        /// </summary>
        public WindowMessageReceiver()
        {
            this.IsDisposed = false;
            this.Lock = new System.Threading.ReaderWriterLock();
            this.CatchMessages = new System.Collections.Generic.HashSet<int>();
            this.Events = new System.Collections.Generic.HashSet<System.Action<System.Windows.Forms.Message>>();
            this.SynchronizationContext = System.ComponentModel.AsyncOperationManager.SynchronizationContext;

            System.Windows.Forms.CreateParams cp = new System.Windows.Forms.CreateParams();
            cp.Caption = GetType().FullName;
            base.CreateHandle(cp);
        }

        /// <summary>
        /// 割り当てられたリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            ClearMessages();
            ClearEvents();
            DestroyHandle();

            this.IsDisposed = true;

            System.GC.Collect();
        }

        /// <summary>
        /// 捕捉するメッセージを追加します。
        /// </summary>
        /// <param name="message">捕捉するメッセージ</param>
        public void RegisterMessage(int message)
        {
            if (this.CatchMessages == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);
            this.CatchMessages.Add(message);
            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// 登録されているメッセージを削除します。
        /// </summary>
        /// <param name="message">削除するメッセージ</param>
        public void RemoveMessage(int message)
        {
            if (this.CatchMessages == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);
            this.CatchMessages.Remove(message);
            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// 登録されているメッセージを全て削除します。
        /// </summary>
        public void ClearMessages()
        {
            if (this.CatchMessages == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);
            this.CatchMessages.Clear();
            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// 捕捉対象メッセージ受信時のイベントを登録します。
        /// </summary>
        /// <param name="receivedEvent">メッセージ受信時のイベント</param>
        public void RegisterEvent(System.Action<System.Windows.Forms.Message> receivedEvent)
        {
            if (this.Events == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);

            if (this.Events.Add(receivedEvent))
            {
                this.ReceivedEvent += receivedEvent;
            }

            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// 登録されているイベントを削除します。
        /// </summary>
        /// <param name="receivedEvent">メッセージ受信時のイベント</param>
        public void RemoveEvent(System.Action<System.Windows.Forms.Message> receivedEvent)
        {
            if (this.Events == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);
            this.ReceivedEvent -= receivedEvent;
            this.Events.Remove(receivedEvent);
            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// 登録されているイベントを全て削除します。
        /// </summary>
        public void ClearEvents()
        {
            if (this.Events == null)
            {
                return;
            }

            this.Lock.AcquireWriterLock(System.Threading.Timeout.Infinite);
            foreach (System.Action<System.Windows.Forms.Message> e in this.Events)
            {
                this.ReceivedEvent -= e;
            }
            this.Events.Clear();
            this.Lock.ReleaseWriterLock();
        }

        /// <summary>
        /// ウィンドウに関連付けられている既定のウィンドウ プロシージャを呼び出します。
        /// </summary>
        /// <param name="message">現在処理中のメッセージ</param>
        protected override void WndProc(ref System.Windows.Forms.Message message)
        {
            this.Lock.AcquireReaderLock(System.Threading.Timeout.Infinite);
            bool isContain = this.CatchMessages.Contains(message.Msg);
            this.Lock.ReleaseReaderLock();

            if (isContain)
            {
                this.SynchronizationContext.Post((state) =>
                {
                    this.ReceivedEvent?.Invoke((System.Windows.Forms.Message)state);
                }, message);
            }

            base.DefWndProc(ref message);
        }
    }
}
