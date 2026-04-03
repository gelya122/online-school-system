import axios from 'axios';

const apiBaseFromEnv = import.meta.env.VITE_API_BASE_URL as string | undefined;
// По умолчанию ходим через относительный `/api`, чтобы Vite proxy работал независимо от порта.
const apiBaseUrl = apiBaseFromEnv ? apiBaseFromEnv.replace(/\/$/, '') : '/api';

const axiosInstance = axios.create({
    baseURL: apiBaseUrl,
    headers: { 'Content-Type': 'application/json' },
});

// Перехватчик для добавления токена (если используется JWT)
axiosInstance.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Перехватчик ответов - обработка ошибок (например, истекший токен)
axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401) {
            // Токен истек или невалиден - очищаем и перенаправляем на логин
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default axiosInstance;

