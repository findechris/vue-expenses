import axios from 'axios'
import store from '@/store'
import { ADD_ALERT, TOGGLE_LOADING, REFRESHTOKEN } from '@/store/_actiontypes'
import router from '@/router/index'

let isRefreshingToken = false
let callbacks = []

const api = axios.create({
    baseURL: process.env.VUE_APP_BASE_URL
})

api.interceptors.request.use((request) => {
    //add authorization header with jwt token to each request
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const user = (store.state as any).account.user
    if (user && user.token) {
        request.headers['Authorization'] = `Bearer ${user.token}`
    }
    updateLoaderTo(true)
    return request
})

api.interceptors.response.use(
    (response) => {
        updateLoaderTo(false)
        return response
    },
    (error) => {
        updateLoaderTo(false)
        let errormessage =
            error.response &&
            error.response.data.errors &&
            error.response.data.errors.Error
                ? error.response.data.errors.Error
                : error.message
        if (error.response && error.response.status === 422) {
            errormessage = ''
            error.response.data.errors.forEach((value) => {
                errormessage += value.toString() + ' '
            })
        } else if (
            error.response &&
            error.response.status === 401 &&
            error.response.headers['token-expired']
        ) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const user = (store.state as any).account.user
            if (user && user.refreshToken) {
                const originalRequest = error.config
                if (!isRefreshingToken) {
                    isRefreshingToken = true
                    store
                        .dispatch(
                            `account/${REFRESHTOKEN}`,
                            {
                                refreshtoken: user.refreshToken,
                                token: user.token
                            },
                            { root: true }
                        )
                        .then(() => {
                            isRefreshingToken = false
                            tokenRefreshed()
                        })
                        .catch(() => {
                            router.push('/login')
                        })
                }

                const retryOriginalRequest = new Promise((resolve) => {
                    addCallback(() => {
                        originalRequest.headers.Authorization = `Bearer ${user.token}`
                        resolve(api(originalRequest))
                    })
                })
                return retryOriginalRequest
            }
        }

        store.dispatch(
            `alert/${ADD_ALERT}`,
            { message: errormessage, color: 'error' },
            { root: true }
        )
        return Promise.reject(error)
    }
)

const updateLoaderTo = (loading) => {
    store.dispatch(
        `loader/${TOGGLE_LOADING}`,
        { loading: loading },
        { root: true }
    )
}

const tokenRefreshed = () => {
    callbacks = callbacks.filter((callback) => callback())
}

const addCallback = (callback) => {
    callbacks.push(callback)
}

export default api
