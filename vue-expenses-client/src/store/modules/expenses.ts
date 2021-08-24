import Api from '@/services/api'
import {
    LOAD_EXPENSES,
    CREATE_EXPENSE,
    EDIT_EXPENSE,
    REMOVE_EXPENSE,
    ADD_ALERT,
    EDIT_STATISTICS,
    REMOVE_EXPENSESOFTYPE,
    REMOVE_EXPENSESOFCATEGORY
} from '@/store/_actiontypes'
import {
    SET_EXPENSES,
    ADD_EXPENSE,
    UPDATE_EXPENSE,
    DELETE_EXPENSE,
    DELETE_EXPENSESOFTYPE,
    DELETE_EXPENSESOFCATEGORY,
    SET_EXPENSES_PAGINATION
} from '@/store/_mutationtypes'
import sumBy from 'lodash/sumBy'
import groupBy from 'lodash/groupBy'
import map from 'lodash/map'
import orderBy from 'lodash/orderBy'

export interface IExpenses {
    id: number
    date: string
    categoryId?: number
    category?: string
    categoryBudget?: number
    categoryColour?: string
    typeId: number
    type: string
    value: number
    comments: string
}

export interface IPagination {
    sortDesc: boolean[]
    page: number
    itemsPerPage: number
    sortBy: string[]
    totalItems: number
    rowsPerPageItems: number[]
}

export interface IExpensesState {
    expenses: IExpenses[]
    pagination: IPagination
}

const state: IExpensesState = {
    expenses: [],
    pagination: {
        sortDesc: [true],
        page: 1,
        itemsPerPage: 10,
        sortBy: ['date'],
        totalItems: 10,
        rowsPerPageItems: [1, 2, 4, 8, 16]
    }
}

const actions = {
    async [LOAD_EXPENSES]({ commit, getters }) {
        const pagination = getters.pagination
        const offset = (pagination.page - 1) * pagination.itemsPerPage
        let sortBy = 'id'
        let sortDesc = true
        if (pagination.sortBy && pagination.sortBy.length > 0) {
            sortBy = pagination.sortBy[0]
            sortDesc = pagination.sortDesc[0]
        }
        const response = await Api.get(
            `/expenses/list/${offset}/${pagination.itemsPerPage}/${sortBy}/${sortDesc}`
        )
        const expenses = response.data.items
        pagination.totalItems = response.data.totalItems
        commit(SET_EXPENSES_PAGINATION, pagination)
        commit(SET_EXPENSES, expenses)
    },
    async [CREATE_EXPENSE]({ commit, dispatch }, { expense }) {
        const response = await Api.post('/expenses', {
            date: expense.date,
            categoryId: expense.categoryId,
            typeId: expense.typeId,
            value: expense.value,
            comments: expense.comments
        })
        const expense_1 = response.data
        commit(ADD_EXPENSE, expense_1)
        dispatch(
            `alert/${ADD_ALERT}`,
            { message: 'Expense added successfully', color: 'success' },
            { root: true }
        )
        dispatch(
            `statistics/${EDIT_STATISTICS}`,
            { expense: expense_1, operation: 'create' },
            { root: true }
        )
    },
    async [EDIT_EXPENSE]({ commit, dispatch }, { expense }) {
        const response = await Api.put(`/expenses/${expense.id}`, expense)
        const expense_1 = response.data
        commit(UPDATE_EXPENSE, expense_1)
        dispatch(
            `alert/${ADD_ALERT}`,
            { message: 'Expense updaded successfully', color: 'success' },
            { root: true }
        )
        dispatch(
            `statistics/${EDIT_STATISTICS}`,
            { expense: expense_1, operation: 'edit' },
            { root: true }
        )
    },
    async [REMOVE_EXPENSE]({ commit, dispatch }, { id }) {
        await Api.delete(`/expenses/${id}`)
        const expense = state.expenses.filter((ec) => ec.id == id)[0]
        commit(DELETE_EXPENSE, id)
        dispatch(
            `alert/${ADD_ALERT}`,
            { message: 'Expense deleted successfully', color: 'success' },
            { root: true }
        )
        dispatch(
            `statistics/${EDIT_STATISTICS}`,
            { expense: expense, operation: 'remove' },
            { root: true }
        )
    },
    [REMOVE_EXPENSESOFTYPE]({ commit }, { typeId }) {
        commit(DELETE_EXPENSESOFTYPE, typeId)
    },
    [REMOVE_EXPENSESOFCATEGORY]({ commit }, { categoryId }) {
        commit(DELETE_EXPENSESOFCATEGORY, categoryId)
    }
}

const mutations = {
    [SET_EXPENSES_PAGINATION](state: IExpensesState, pagination: IPagination) {
        state.pagination = pagination
    },
    [SET_EXPENSES](state: IExpensesState, expenses: IExpenses[]) {
        state.expenses = expenses
    },
    [ADD_EXPENSE](state: IExpensesState, expense: IExpenses) {
        state.expenses.push(expense)
    },
    [UPDATE_EXPENSE](state: IExpensesState, expense: IExpenses) {
        const expenseUpdated = state.expenses.find((ec) => ec.id == expense.id)
        expenseUpdated.date = expense.date
        expenseUpdated.value = expense.value
        expenseUpdated.categoryId = expense.categoryId
        expenseUpdated.category = expense.category
        expenseUpdated.typeId = expense.typeId
        expenseUpdated.type = expense.type
        expenseUpdated.comments = expense.comments
    },
    [DELETE_EXPENSE](state: IExpensesState, id: number) {
        state.expenses = state.expenses.filter((ec) => ec.id != id)
    },
    [DELETE_EXPENSESOFTYPE](state: IExpensesState, typeId: number) {
        state.expenses = state.expenses.filter((ec) => ec.typeId != typeId)
    },
    [DELETE_EXPENSESOFCATEGORY](state: IExpensesState, categoryId: number) {
        state.expenses = state.expenses.filter(
            (ec) => ec.categoryId != categoryId
        )
    }
}

const getters = {
    pagination: (state) => {
        return state.pagination
    },
    overallSpent: (state, getters, rootState) => {
        const overallSpent = new Intl.NumberFormat(
            window.navigator.language
        ).format(sumBy(state.expenses, 'value').toFixed(2))
        return `${rootState.account.user.displayCurrency} ${overallSpent}`
    },
    mostSpentBy: (state) => {
        return state.expenses.length <= 0
            ? 'N/A'
            : orderBy(
                  map(
                      groupBy(state.expenses, (e) => {
                          return e.type
                      }),
                      (type, id) => ({
                          type: id,
                          value: sumBy(type, 'value')
                      })
                  ),
                  ['value'],
                  ['desc']
              )[0].type
    },
    mostSpentOn: (state) => {
        return state.expenses.length <= 0
            ? 'N/A'
            : orderBy(
                  map(
                      groupBy(state.expenses, (e) => {
                          return e.category
                      }),
                      (category, id) => ({
                          category: id,
                          value: sumBy(category, 'value')
                      })
                  ),
                  ['value'],
                  ['desc']
              )[0].category
    },
    spentThisYear: (state, getters, rootState) => {
        const currentYear = new Date().getFullYear()
        const spentThisYear = new Intl.NumberFormat(
            window.navigator.language
        ).format(
            sumBy(
                state.expenses.filter((o) => {
                    return new Date(o.date).getFullYear() == currentYear
                }),
                'value'
            ).toFixed(2)
        )
        return `${rootState.account.user.displayCurrency} ${spentThisYear}`
    }
}

export const expenses = {
    namespaced: true,
    state,
    actions,
    mutations,
    getters
}
