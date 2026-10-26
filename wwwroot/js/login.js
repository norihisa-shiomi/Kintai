// 入力検証・API送信・画面遷移用スクリプト
const ERROR_MESSAGES = {
    "E-0001_EMAIL": "メールアドレスを入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0001_PASSWORD": "パスワードを入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0002": "パスワードは指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0003": "メールアドレスは指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0004": "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0005": "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。"
};

const formLogin = document.getElementById("form-login");
const inputEmail = document.getElementById("input-email");
const inputPassword = document.getElementById("input-password");
const errorEmail = document.getElementById("error-email");
const errorPassword = document.getElementById("error-password");
const alertError = document.getElementById("alert-error");
const btnLogin = document.getElementById("btn-login");

formLogin.addEventListener("submit", async (e) => {
    e.preventDefault();

    // エラー表示の初期化
    errorEmail.classList.add("d-none");
    errorEmail.textContent = "";
    errorPassword.classList.add("d-none");
    errorPassword.textContent = "";
    alertError.classList.add("d-none");
    alertError.textContent = "";

    const email = inputEmail.value.trim();
    const password = inputPassword.value;
    let hasError = false;

    // メールアドレス 入力チェック (E-0001 必須, E-0003 形式)
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) {
        errorEmail.textContent = ERROR_MESSAGES["E-0001_EMAIL"];
        errorEmail.classList.remove("d-none");
        hasError = true;
    } else if (!emailRegex.test(email)) {
        errorEmail.textContent = ERROR_MESSAGES["E-0003"];
        errorEmail.classList.remove("d-none");
        hasError = true;
    }

    // パスワード 入力チェック (E-0001 必須, E-0002 桁数, E-0004 英数記号形式)
    const passwordComplexityRegex = /^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$/;
    if (!password) {
        errorPassword.textContent = ERROR_MESSAGES["E-0001_PASSWORD"];
        errorPassword.classList.remove("d-none");
        hasError = true;
    } else if (password.length < 8 || password.length > 32) {
        errorPassword.textContent = ERROR_MESSAGES["E-0002"];
        errorPassword.classList.remove("d-none");
        hasError = true;
    } else if (!passwordComplexityRegex.test(password)) {
        errorPassword.textContent = ERROR_MESSAGES["E-0004"];
        errorPassword.classList.remove("d-none");
        hasError = true;
    }

    if (hasError) return;

    // ボタンの多重送信防止
    btnLogin.disabled = true;

    try {
        // 認証処理の呼び出し (LoginController へ POST)
        const response = await fetch("/api/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });

        const result = await response.json();

        // 画面遷移 (ログイン成功判定)
        if (response.ok && result.isSuccess) {
            if (result.isFirstLogin) {
                // 初回ログイン時は 初回ログインへ遷移
                window.location.href = "/firstlogin.html";
            } else {
                // 通常ログイン成功時は 打刻画面へ遷移
                window.location.href = "/timestamp.html";
            }
        } else {
            // 認証エラー (E-0005) の赤文字表示
            const code = result.errorCode || "E-0005";
            alertError.textContent = ERROR_MESSAGES[code] || ERROR_MESSAGES["E-0005"];
            alertError.classList.remove("d-none");
        }
    } catch (err) {
        alertError.textContent = ERROR_MESSAGES["E-0005"];
        alertError.classList.remove("d-none");
    } finally {
        btnLogin.disabled = false;
    }
});