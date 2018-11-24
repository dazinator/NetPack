function process(count) {
    var value = "";
    value += count; //1. Two consecutive += statements
    value += count;
    value += count;
    count++;        //2. Some other statement
    return value;   //3. Return
};
 
 