(module
    (import "js" "mem" (memory 1))
    (import "js" "write" (func (param i32 i32)))
    (data (i32.const 0) "asd")
    
    ;; (import "js" "read" (func (param i32) (result i32)))
    (func (result i32)
        i32.const 24)

    (func (local i32 i32 i32)
        i32.const 24
        i32.const 1234
        i32.store)
    (func (export "__main__") (local i32)
        i32.const 332
        drop))