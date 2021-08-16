import Api from '@/services/api'
import { LOAD_USERS } from '../_actiontypes'
import { SET_USERS } from '../_mutationtypes'

const state = {
    types: []
}

const actions = {
    async [LOAD_USERS]({ commit }) {
        const response = await Api.get('/users')
        const users = response.data
        commit(SET_USERS, users)
    }
}

const mutations = {
    [SET_USERS](state: any, users: any) {
        state.users = users
    }
}

export const users = {
    namespaced: true,
    state,
    actions,
    mutations
}
