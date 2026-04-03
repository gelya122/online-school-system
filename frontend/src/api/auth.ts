import axiosInstance from './axiosInstance';
import { LoginCredentials, RegisterData, AuthResponse } from '../types';

export const login = (data: LoginCredentials) => 
  axiosInstance.post<AuthResponse>('/auth/login', data);

export const register = (data: RegisterData) => 
  axiosInstance.post<AuthResponse>('/auth/register', data);

export const logout = () => 
  axiosInstance.post('/auth/logout');

