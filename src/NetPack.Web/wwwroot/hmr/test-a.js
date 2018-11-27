import { module } from '@hot';

export const _state = module ? module._state : {};

export const __unload = () => {
    console.log('Unload a a something (unsubscribe from listeners, disconnect from socket, etc...)')
    // force unload React components
    // ReactDOM.unmountComponentAtNode(DOMNode);	// your container node
};

import('./dynamically-imported/apply-color-and-message').then(({ default: apply }) => {
    apply('#a [data-used-by="a"]', usedByA);
});

export class q {
    constructor() {
        this.es6 = 'yay';
    }
}