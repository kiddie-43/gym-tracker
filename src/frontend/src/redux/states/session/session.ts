import { PopUpCode } from '../../../enums/popUp/popUp';
import { ISession, ISessionFilter } from '../../../interfaces/ISession/ISession';
import { IReduxState } from '../../../interfaces/skeleton/IRedux/IReduxState';
import type { RootState } from '../../store';

export const sessionsInitialState: IReduxState<ISessionFilter, ISession> = {
    table: {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 10,
        sortBy: '',
        sortDirection: 'asc',
    },

    filters: {

    },
    form: {
        name: '',
        daysOfWeek: [],
        id: ''
    },
    error: null,
    loading: false,
    popUpCode: PopUpCode.Default,
    message: null
};

export const selectSessionsState = (state: RootState) => state.sessions;
export const selectSessionsForm = (state: RootState) => state.sessions.form;
