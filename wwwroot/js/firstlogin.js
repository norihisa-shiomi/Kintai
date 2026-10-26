// 入力検証・API送信・画面遷移用スクリプト (SC-02 初回ログイン)
const ERROR_MESSAGES = {
    "E-0001_EMAIL": "メールアドレスを入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0001_NEW_PASSWORD": "初回パスワードを入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0001_CONFIRM_PASSWORD": "確認用パスワードを入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0002": "パスワードは指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0003": "メールアドレスは指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0004": "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。",
    "E-0005": "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。",
    "E-0006": "パスワードと確認用パスワードが一致していません。両方に同じパスワードを入力してください。"
};

const formFirstLogin = document.getElementById("form-first-login");
const inputEmail = document.getElementById("input-email");
const inputNewPassword = document.getElementById("input-new-password");
const inputConfirmPassword = document.getElementById("input-confirm-password");
const errorEmail = document.getElementById("error-email");
const errorNewPassword = document.getElementById("error-new-password");
const errorConfirmPassword = document.getElementById("error-confirm-password");
const alertError = document.getElementById("alert-error");
const btnFirstLogin = document.getElementById("btn-first-login");

formFirstLogin.addEventListener("submit", async (e) => {
    e.preventDefault();

    // エラー表示の初期化
    errorEmail.classList.add("d-none");
    errorEmail.textContent = "";
    errorNewPassword.classList.add("d-none");
    errorNewPassword.textContent = "";
    errorConfirmPassword.classList.add("d-none");
    errorConfirmPassword.textContent = "";
    alertError.classList.add("d-none");
    alertError.textContent = "";

    const email = inputEmail.value.trim();
    const newPassword = inputNewPassword.value;
    const confirmPassword = inputConfirmPassword.value;
    let hasError = false;

    // 設計: 1. メールアドレス 入力チェック (E-0001 必須, E-0003 形式)
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

    // 設計: 2. 初回パスワード 入力チェック (E-0001 必須, E-0002 桁数, E-0004 英数記号形式)
    const passwordComplexityRegex = /^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$/;
    if (!newPassword) {
        errorNewPassword.textContent = ERROR_MESSAGES["E-0001_NEW_PASSWORD"];
        errorNewPassword.classList.remove("d-none");
        hasError = true;
    } else if (newPassword.length < 8 || newPassword.length > 32) {
        errorNewPassword.textContent = ERROR_MESSAGES["E-0002"];
        errorNewPassword.classList.remove("d-none");
        hasError = true;
    } else if (!passwordComplexityRegex.test(newPassword)) {
        errorNewPassword.textContent = ERROR_MESSAGES["E-0004"];
        errorNewPassword.classList.remove("d-none");
        hasError = true;
    }

    // 設計: 3. 初回パスワード確認 入力チェック (E-0001 必須, E-0006 不一致)
    if (!confirmPassword) {
        errorConfirmPassword.textContent = ERROR_MESSAGES["E-0001_CONFIRM_PASSWORD"];
        errorConfirmPassword.classList.remove("d-none");
        hasError = true;
    } else if (newPassword !== confirmPassword) {
        errorConfirmPassword.textContent = ERROR_MESSAGES["E-0006"];
        errorConfirmPassword.classList.remove("d-none");
        hasError = true;
    }

    if (hasError) return;

    // ボタンの二重送信防止
    btnFirstLogin.disabled = true;

    try {
        // 設計: 初回ログイン・登録 API 呼び出し (POST: api/firstlogin)
        const response = await fetch("/api/firstlogin", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email: email,
                newPassword: newPassword,
                confirmPassword: confirmPassword
            })
        });

        const result = await response.json();

        // 設計: 画面遷移 (SC-02 初回ログイン成功時 ⇒ SC-04 打刻画面へ遷移)
        if (response.ok && result.isSuccess) {
            window.location.href = result.redirectUrl || "/timestamp.html";
        } else {
            // 設計: 5. 業務エラーメッセージ (E-0005 認証エラー等の表示)
            const code = result.errorCode || "E-0005";
            alertError.textContent = ERROR_MESSAGES[code] || result.message || ERROR_MESSAGES["E-0005"];
            alertError.classList.remove("d-none");
        }
    } catch (err) {
        alertError.textContent = ERROR_MESSAGES["E-0005"];
        alertError.classList.remove("d-none");
    } finally {
        btnFirstLogin.disabled = false;
    }
});