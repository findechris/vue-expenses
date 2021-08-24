import Vue from 'vue'
import Vuex from 'vuex'
import { alert } from './modules/alert'
import { loader } from './modules/loader'
import { account } from './modules/account'
import { expenseTypes } from './modules/expensetypes'
import { expenseCategories } from './modules/expensecategories'
import { expenses, IExpensesState } from './modules/expenses'
import { statistics } from './modules/statistics'
import { users } from './modules/users'
import createPersistedState from 'vuex-persistedstate'
import SecureLS from 'secure-ls'
const ls = new SecureLS({ isCompression: false })

Vue.use(Vuex)
export interface IUserStateInterface {
    users: any
}

export interface IStateInterface {
    // Define your own store structure, using submodules if needed
    // example: ExampleStateInterface;
    // Declared as unknown to avoid linting issue. Best to strongly type as per the line above.
    users: IUserStateInterface
    expenses: IExpensesState
}

const store = new Vuex.Store<IStateInterface>({
    modules: {
        alert,
        loader,
        account,
        expenseTypes,
        expenseCategories,
        expenses,
        statistics,
        users
    },
    plugins: [
        createPersistedState({
            storage: {
                getItem: (key) => ls.get(key),
                setItem: (key, value) => ls.set(key, value),
                removeItem: (key) => ls.remove(key)
            }
        })
    ]
})

export default store
